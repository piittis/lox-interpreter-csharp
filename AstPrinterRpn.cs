using System;
using System.Collections.Generic;
using System.Text;

namespace Lox
{
    /// <summary>
    /// generates a string representation in revese polish notation for different expressions
    /// </summary>
    class AstPrinterRpn : IVisitor<string>
    {

        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return $"{expr.left.Accept(this)} {expr.right.Accept(this)} {expr.op.lexeme}";
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return expr.expression.Accept(this);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value.ToString();
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return $"{expr.op.lexeme}{expr.right.Accept(this)}";
        }
    }
}
