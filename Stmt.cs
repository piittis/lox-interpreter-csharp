namespace Lox
{
	using System.Collections.Generic;

	abstract class Stmt 
	{ 
		public abstract T Accept<T>(IVisitor<T> visitor);

        public interface IVisitor<T> {
            T VisitExpressionStmt(Expression stmt);
            T VisitIfStmt(If stmt);
            T VisitPrintStmt(Print stmt);
            T VisitWhileStmt(While stmt);
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

        public class If : Stmt {

            public Expr condition;
            public Stmt thenBranch;
            public Stmt elseBranch;

            public If (Expr condition, Stmt thenBranch, Stmt elseBranch) {
                this.condition = condition;
                this.thenBranch = thenBranch;
                this.elseBranch = elseBranch;
            }

            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitIfStmt(this); }

	    }

        public class Print : Stmt {

            public Expr expression;

            public Print (Expr expression) {
                this.expression = expression;
            }

            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitPrintStmt(this); }

	    }

        public class While : Stmt {

            public Expr condition;
            public Stmt body;

            public While (Expr condition, Stmt body) {
                this.condition = condition;
                this.body = body;
            }

            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitWhileStmt(this); }

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