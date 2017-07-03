using System;
using System.Collections.Generic;
using static Lox.TokenType;

namespace Lox
{
    class Interpreter : Expr.IVisitor<Object>, Stmt.IVisitor<Object>
    {

        private Environment environment = new Environment();

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            } catch (RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
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
                    if (left is double && right is double) {
                        return (double)left + (double)right;
                    }

                    if (left is string || right is string) {
                        return left.ToString() + right.ToString();
                    }

                    throw new RuntimeError(expr.op, "Operands must be numbers or one of them must be a string");

                case BANG_EQUAL: return !IsEqual(left, right);
                case EQUAL_EQUAL: return IsEqual(left, right);
            }

            // Unreachable.
            return null;
        }

        public object VisitCommaExpr(Expr.Comma expr)
        {
            Evaluate(expr.left);
            return Evaluate(expr.right);
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitTernaryExpr(Expr.Ternary expr)
        {
            if (IsTrue(Evaluate(expr.condition)))
            {
                return Evaluate(expr.ifTrue);
            }
            return Evaluate(expr.ifFalse);
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch (expr.op.type)
            {
                case BANG:
                    return !IsTrue(right);
                case MINUS:
                    TypeCheckNumberOperand(expr.op, right);
                    return -(double)right;
            }

            // Unreachable.
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return environment.Get(expr.name);
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);
            environment.Assign(expr.name, value);
            return value;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            Console.WriteLine(Stringify(Evaluate(stmt.expression)));
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            Object value = null;
            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }
            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements);
            return null;
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void ExecuteBlock(List<Stmt> statements)
        {
            // Create new scope for the block and discard it when done.
            Environment prev = environment;
            try
            {
                environment = new Environment(environment);
                foreach(var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                environment = prev;
            }
        }

        private bool IsTrue(object obj)
        {
            // C# 7 pattern matching
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
