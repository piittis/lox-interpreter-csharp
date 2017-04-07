using System.Text;

namespace Lox
{
    /// <summary>
    /// generates a string representation for different expressions
    /// </summary>
    class AstPrinter : IVisitor<string>
    {

        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.left, expr.right);
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.expression);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value.ToString();
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.right);
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
