using System;
using static Lox.TokenType;

namespace Lox
{
    class Interpreter : IVisitor<Object>
    {

        public void Interpret(Expr expression)
        {
            try
            {
                object value = Evaluate(expression);
                Console.WriteLine(Stringify(value));
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
            if (IsTrue(Evaluate(expr.cond)))
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

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
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
