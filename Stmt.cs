namespace Lox
{
	using System.Collections.Generic;

	abstract class Stmt 
	{ 
		public abstract T Accept<T>(IVisitor<T> visitor);

        public interface IVisitor<T> {
            T VisitExpressionStmt(Expression stmt);
            T VisitPrintStmt(Print stmt);
            T VisitVarStmt(Var stmt);
            T VisitBlockStmt(Block stmt);
        }

        public class Expression : Stmt {

            public Expr expression;

            public Expression (Expr expression) {
                this.expression = expression;
            }

            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitExpressionStmt(this); }

	    }

        public class Print : Stmt {

            public Expr expression;

            public Print (Expr expression) {
                this.expression = expression;
            }

            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitPrintStmt(this); }

	    }

        public class Var : Stmt {

            public Token name;
            public Expr initializer;

            public Var (Token name, Expr initializer) {
                this.name = name;
                this.initializer = initializer;
            }

            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitVarStmt(this); }

	    }

        public class Block : Stmt {

            public List<Stmt> statements;

            public Block (List<Stmt> statements) {
                this.statements = statements;
            }

            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitBlockStmt(this); }

	    }
    }

}