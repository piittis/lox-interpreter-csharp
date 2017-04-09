namespace Lox
{
    abstract class Expr 
    { 
        public abstract T Accept<T>(IVisitor<T> visitor);

        public class Binary : Expr {

            public Expr left;
            public Token op;
            public Expr right;

            public Binary (Expr left, Token op, Expr right) {
                this.left = left;
                this.op = op;
                this.right = right;
            }

            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitBinaryExpr(this); }

	    }

        public class Grouping : Expr {

            public Expr expression;

            public Grouping (Expr expression) {
                this.expression = expression;
            }

            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitGroupingExpr(this); }

	    }

        public class Literal : Expr {

            public object value;

            public Literal (object value) {
                this.value = value;
            }

            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitLiteralExpr(this); }

	    }

        public class Unary : Expr {

            public Token op;
            public Expr right;

            public Unary (Token op, Expr right) {
                this.op = op;
                this.right = right;
            }

            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitUnaryExpr(this); }

	    }

        public class Comma : Expr {

            public Expr left;
            public Expr right;

            public Comma (Expr left, Expr right) {
                this.left = left;
                this.right = right;
            }

            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitCommaExpr(this); }

	    }

        public class Ternary : Expr {

            public Expr cond;
            public Expr ifTrue;
            public Expr ifFalse;

            public Ternary (Expr cond, Expr ifTrue, Expr ifFalse) {
                this.cond = cond;
                this.ifTrue = ifTrue;
                this.ifFalse = ifFalse;
            }

            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitTernaryExpr(this); }

	    }
    }

    interface IVisitor<T> {
        T VisitBinaryExpr(Expr.Binary expr);
        T VisitGroupingExpr(Expr.Grouping expr);
        T VisitLiteralExpr(Expr.Literal expr);
        T VisitUnaryExpr(Expr.Unary expr);
        T VisitCommaExpr(Expr.Comma expr);
        T VisitTernaryExpr(Expr.Ternary expr);
    }
}