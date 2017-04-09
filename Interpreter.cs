using System;
using System.Collections.Generic;
using System.Text;
using static Lox.TokenType;

namespace Lox
{
    class Interpreter : IVisitor<Object>
    {

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            Object left = Evaluate(expr.left);
            Object right = Evaluate(expr.right);

            switch (expr.op.type)
            {
                case GREATER:
                    return (double)left > (double)right;
                case GREATER_EQUAL:
                    return (double)left >= (double)right;
                case LESS:
                    return (double)left < (double)right;
                case LESS_EQUAL:
                    return (double)left <= (double)right;
                case MINUS:
                    return (double)left - (double)right;
                case SLASH:
                    return (double)left / (double)right;
                case STAR:
                    return (double)left * (double)right;
                case PLUS:
                    if (left is double && right is double) {
                        return (double)left + (double)right;
                    }

                    if (left is String && right is String) {
                        return (string)left + (string)right;
                    }
                    break;
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

    }
}
