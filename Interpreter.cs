using System;
using System.Collections.Generic;
using System.Linq;
using static Lox.TokenType;

namespace Lox
{
    class Interpreter : Expr.IVisitor<Object>, Stmt.IVisitor<Object>
    {

        public readonly Environment globals = new Environment();
        private Environment environment;
        // How many scopes are between variable reference and the variable itself. Resolved beforehand.
        private readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();

        // For debugging:
        private List<Stmt> statements;
        private Stmt currentStatement;

        public Interpreter()
        {
            environment = globals;
        }

        public void Interpret(List<Stmt> statements)
        {
            this.statements = statements;
            try
            {
                foreach (var statement in statements)
                {
                    this.currentStatement = statement;
                    Execute(statement);
                }
            } catch (RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        public void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public void ExecuteBlock(List<Stmt> statements, Environment blockEnvironment)
        {
            Environment previous = environment;
            try
            {
                environment = blockEnvironment;
                statements.ForEach(Execute);
            }
            finally
            {
                environment = previous;
            }
        }

        public void Resolve(Expr expr, int depth)
        {
            locals.Add(expr, depth);
        }

        public string EvaluateExpr(Expr expr)
        {
            try
            {
                return Stringify(Evaluate(expr));
            }
            catch (RuntimeError error)
            {
                Lox.RuntimeError(error);               
            }
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            object value = Environment.unAssigned;
            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }
            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }
            return null;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            LoxFunction function = new LoxFunction(stmt, environment, false);
            environment.Define(stmt.name.lexeme, function);
            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            Console.WriteLine(Stringify(Evaluate(stmt.expression)));
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if (stmt.value != null) value = Evaluate(stmt.value);

            throw new Return(value);
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(environment));
            return null;
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            environment.Define(stmt.name.lexeme, null);

            var methods = stmt.methods.ToDictionary(m => m.name.lexeme, m => new LoxFunction(m, environment, m.name.lexeme == "this"));
            LoxClass klass = new LoxClass(stmt.name.lexeme, methods);

            environment.Assign(stmt.name, klass);
            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);
            if (locals.TryGetValue(expr, out int distance))
            {
                environment.AssignAt(distance, expr.name, value);
            }
            else
            {
                environment.Assign(expr.name, value);
            }
            
            return value;
        }

        public object VisitCommaExpr(Expr.Comma expr)
        {
            Evaluate(expr.left);
            return Evaluate(expr.right);
        }

        public object VisitTernaryExpr(Expr.Ternary expr)
        {
            if (IsTruthy(Evaluate(expr.condition)))
            {
                return Evaluate(expr.ifTrue);
            }
            return Evaluate(expr.ifFalse);
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            Object left = Evaluate(expr.left);
            Object right = Evaluate(expr.right);

            switch (expr.op.type)
            {
                case GREATER:
                    TypeCheckNumberOperands(expr.op, left, right);
                    return (double)left > (double)right;
                case GREATER_EQUAL:
                    TypeCheckNumberOperands(expr.op, left, right);
                    return (double)left >= (double)right;
                case LESS:
                    TypeCheckNumberOperands(expr.op, left, right);
                    return (double)left < (double)right;
                case LESS_EQUAL:
                    TypeCheckNumberOperands(expr.op, left, right);
                    return (double)left <= (double)right;
                case MINUS:
                    TypeCheckNumberOperands(expr.op, left, right);
                    return (double)left - (double)right;
                case SLASH:
                    TypeCheckNumberOperands(expr.op, left, right);
                    if ((double)right == 0) throw new RuntimeError(expr.op, "Division by zero.");
                    return (double)left / (double)right;
                case STAR:
                    TypeCheckNumberOperands(expr.op, left, right);
                    return (double)left * (double)right;
                case PLUS:
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }

                    if (left is string || right is string)
                    {
                        return left.ToString() + right.ToString();
                    }

                    throw new RuntimeError(expr.op, "Operands must be numbers or one of them must be a string");

                case BANG_EQUAL: return !IsEqual(left, right);
                case EQUAL_EQUAL: return IsEqual(left, right);
            }

            // Unreachable.
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            object callee = Evaluate(expr.callee);

            var arguments = expr.arguments.Select(Evaluate).ToList();

            if (callee is ICallable func)
            {
                if (arguments.Count != func.Arity())
                {
                    throw new RuntimeError(expr.paren,
                        $"Expected {func.Arity()} arguments but got {arguments.Count}.");
                }

                return func.Call(this, arguments);
            }
            throw new RuntimeError(expr.paren, "Can only call functions and classes.");
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            if (Evaluate(expr.obj) is LoxInstance instance)
            {
                return instance[expr.name];
            }

            throw new RuntimeError(expr.name, "Only instances have properties");
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch (expr.op.type)
            {
                case BANG:
                    return !IsTruthy(right);
                case MINUS:
                    TypeCheckNumberOperand(expr.op, right);
                    return -(double)right;
            }

            // Unreachable.
            return null;
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            // Return value is guaranteed to be truthy for truthy expression and vice versa for falsy. 
            // Not necessarily true or false.
            object left = Evaluate(expr.left);

            if (expr.op.type == OR)
            {
                if (IsTruthy(left))
                {
                    return left;
                }
            }
            else
            {
                if (!IsTruthy(left))
                {
                    return left;
                }
            }

            return Evaluate(expr.right);
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            object obj = Evaluate(expr.obj);

            if (obj is LoxInstance instance)
            {
                object value = Evaluate(expr.value);
                instance[expr.name] = value;
                return value;
            }

            throw new RuntimeError(expr.name, "Only instances have fields.");
        }

        public object VisitThisExpr(Expr.This expr)
        {
            return LookupVariable(expr.keyword, expr);
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return LookupVariable(expr.name, expr);
        }

        private object LookupVariable(Token name, Expr expr)
        {
            if (locals.TryGetValue(expr, out int distance))
            {
                return environment.GetAt(distance, name);
            }
            else
            {
                return globals.Get(name);
            }
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private bool IsTruthy(object obj)
        {
            switch (obj)
            {
                case bool b:
                    return b;
                case null:
                    return false;
                default:
                    return true;
            }
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return false;
            if (a == null) return false;

            return a.Equals(b);
        }

        private void TypeCheckNumberOperand(Token op, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(op, "Operand must be a number.");
        }

        private void TypeCheckNumberOperands(Token op, object left, object right)
        {
            if (left is double && right is double) return;
            throw new RuntimeError(op, "Operands must be numbers.");
        }

        private string Stringify(object obj)
        {
            if (obj == null) return "nil";

            return obj.ToString();
        }

    }
}
