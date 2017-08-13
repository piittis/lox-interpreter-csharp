using System;
using System.Text;

namespace Lox
{
    /// <summary>
    /// generates a string representation for different expressions
    /// </summary>
    class AstPrinter : Expr.IVisitor<string>
    {

        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string VisitAssignExpr(Expr.Assign expr)
        {
            return $"({expr.name} <- {expr.value.Accept(this)})";
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.left, expr.right);
        }

        public string VisitCallExpr(Expr.Call expr)
        {
            throw new NotImplementedException();
        }

        public string VisitCommaExpr(Expr.Comma expr)
        {
            return Parenthesize("comma", expr.left, expr.right);
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.expression);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value.ToString();
        }

        public string VisitLogicalExpr(Expr.Logical expr)
        {
            throw new NotImplementedException();
        }

        public string VisitTernaryExpr(Expr.Ternary expr)
        {
            return $"({expr.condition.Accept(this)} ? {expr.ifTrue.Accept(this)} : {expr.ifFalse.Accept(this)})";
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.right);
        }

        public string VisitVariableExpr(Expr.Variable expr)
        {
            return $"({expr.name})";
        }

        private string Parenthesize(string name, params Expr[] exprs)
        {
            var b = new StringBuilder();

            b.Append("(").Append(name);
            foreach(Expr expr in exprs)
            {
                b.Append(" ").Append(expr.Accept(this));
            }
            b.Append(")");
            return b.ToString();
        }
    }
}
