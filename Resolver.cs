using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lox
{
    /// <summary>
    /// Used to staticly resolve variable references before running the actual interpreter.
    /// </summary>
    class Resolver : Expr.IVisitor<Object>, Stmt.IVisitor<Object>
    {
        private readonly Interpreter interpreter;

        /* List represents LIFO listing of nested scopes. Not using Stack because we need to index into it.
         * Dict key is variable name, value tells is it defined or not. False -> is declared, True -> is declared and defined.
         * That info is needed to handle some corner cases like "var foo = foo"
         * */
        private readonly List<ConcurrentDictionary<string, bool>> scopes = new List<ConcurrentDictionary<string, bool>>();

        private ConcurrentDictionary<string, bool> CurrentScope => scopes.Last();
        // Used to track if we are in a specific type of function. We can then report illegal operations.
        private FunctionType currentFunction = FunctionType.NONE;
        private ClassType currentClass = ClassType.NONE;

        private enum FunctionType
        {
            NONE,
            FUNCTION,
            METHOD
        };

        private enum ClassType
        {
            NONE,
            CLASS
        };

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public void Resolve(List<Stmt> statements)
        {
            statements.ForEach(Resolve);
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return null;
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);

            ClassType enclosingClass = currentClass;
            currentClass = ClassType.CLASS;

            BeginScope();
            CurrentScope.TryAdd("this", true);

            stmt.methods.ForEach(m => ResolveFunction(m, FunctionType.METHOD));

            EndScope();
            currentClass = ClassType.CLASS;

            return null;
        }

        public object VisitThisExpr(Expr.This expr)
        {
            if (currentClass == ClassType.NONE)
            {
                Lox.Error(expr.keyword, "Cannot use 'this' outside of a class.");
                return null;
            }
            // When "this" is encountered, we just look it up like it was any other variable.
            ResolveLocal(expr, expr.keyword);
            return null;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);

            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) Resolve(stmt.elseBranch);
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                Lox.Error(stmt.keyword, "Return statement must be inside a function.");
            }
            if (stmt.value != null) Resolve(stmt.value);
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.name);
            if(stmt.initializer != null)
            {
                Resolve(stmt.initializer);
            }
            Define(stmt.name);

            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);
            return null;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.callee);
            expr.arguments.ForEach(Resolve);
            return null;
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr.obj);
            return null;
        }

        public object VisitCommaExpr(Expr.Comma expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.expression);
            return null;
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr.value);
            Resolve(expr.obj);
            return null;
        }

        public object VisitTernaryExpr(Expr.Ternary expr)
        {
            Resolve(expr.condition);
            Resolve(expr.ifTrue);
            Resolve(expr.ifFalse);
            return null;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.right);
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            if (scopes.Count > 0 && CurrentScope.TryGetValue(expr.name.lexeme, out bool val)) {
                if (!val)
                {
                    // Variable is declared but not defined yet.
                    Lox.Error(expr.name, "Cannot read local variable in its own initializer.");
                }
            }

            ResolveLocal(expr, expr.name);
            return null;
        }

        private void Declare(Token name)
        {
            if (scopes.Count == 0) return;
            if (!CurrentScope.TryAdd(name.lexeme, false))
            {
                Lox.Error(name, $"Variable with name '{name.lexeme}' already declared in this scope.");
            }
        }

        private void Define(Token name)
        {
            if (scopes.Count == 0) return;
            CurrentScope.AddOrUpdate(name.lexeme, false, (a, b) => true);
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            // Try to resolve a variable from any scope starting from the innermost.
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes[i].ContainsKey(name.lexeme))
                {
                    // Tell the interpreter where the variable is found.
                    interpreter.Resolve(expr, scopes.Count - 1 - i);
                }
            }

            // Not found. Assume it is global.
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            BeginScope();
            foreach (var param in function.parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.body);
            EndScope();
            currentFunction = enclosingFunction;
        }

        private void BeginScope()
        {
            scopes.Add(new ConcurrentDictionary<string, bool>());
        }

        private void EndScope()
        {
            scopes.RemoveAt(scopes.Count - 1);
        }

    }
}
