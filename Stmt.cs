namespace Lox
{
    using System.Collections.Generic;
    
    abstract class Stmt
    {
        public abstract T Accept<T>(IVisitor<T> visitor);
        
        public interface IVisitor<T> {
        
            T VisitExpressionStmt(Expression stmt);
            T VisitFunctionStmt(Function stmt);
            T VisitIfStmt(If stmt);
            T VisitPrintStmt(Print stmt);
            T VisitReturnStmt(Return stmt);
            T VisitWhileStmt(While stmt);
            T VisitVarStmt(Var stmt);
            T VisitBlockStmt(Block stmt);
            T VisitClassStmt(Class stmt);  
               
        }
        
        
        public class Expression : Stmt
        {
            public readonly Expr expression;
            
            public Expression ( Expr expression)
            {
                this.expression = expression;                
            }
            
            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitExpressionStmt(this); }
        }
        public class Function : Stmt
        {
            public readonly Token name;
            public readonly List<Token> parameters;
            public readonly List<Stmt> body;
            public readonly bool isStatic;
            public readonly bool isGetter;
            
            public Function ( Token name, List<Token> parameters, List<Stmt> body, bool isStatic, bool isGetter)
            {
                this.name = name;
                this.parameters = parameters;
                this.body = body;
                this.isStatic = isStatic;
                this.isGetter = isGetter;                
            }
            
            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitFunctionStmt(this); }
        }
        public class If : Stmt
        {
            public readonly Expr condition;
            public readonly Stmt thenBranch;
            public readonly Stmt elseBranch;
            
            public If ( Expr condition, Stmt thenBranch, Stmt elseBranch)
            {
                this.condition = condition;
                this.thenBranch = thenBranch;
                this.elseBranch = elseBranch;                
            }
            
            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitIfStmt(this); }
        }
        public class Print : Stmt
        {
            public readonly Expr expression;
            
            public Print ( Expr expression)
            {
                this.expression = expression;                
            }
            
            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitPrintStmt(this); }
        }
        public class Return : Stmt
        {
            public readonly Token keyword;
            public readonly Expr value;
            
            public Return ( Token keyword, Expr value)
            {
                this.keyword = keyword;
                this.value = value;                
            }
            
            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitReturnStmt(this); }
        }
        public class While : Stmt
        {
            public readonly Expr condition;
            public readonly Stmt body;
            
            public While ( Expr condition, Stmt body)
            {
                this.condition = condition;
                this.body = body;                
            }
            
            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitWhileStmt(this); }
        }
        public class Var : Stmt
        {
            public readonly Token name;
            public readonly Expr initializer;
            
            public Var ( Token name, Expr initializer)
            {
                this.name = name;
                this.initializer = initializer;                
            }
            
            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitVarStmt(this); }
        }
        public class Block : Stmt
        {
            public readonly List<Stmt> statements;
            
            public Block ( List<Stmt> statements)
            {
                this.statements = statements;                
            }
            
            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitBlockStmt(this); }
        }
        public class Class : Stmt
        {
            public readonly Token name;
            public readonly List<Stmt.Function> methods;
            
            public Class ( Token name, List<Stmt.Function> methods)
            {
                this.name = name;
                this.methods = methods;                
            }
            
            public override T Accept<T>(IVisitor<T> visitor) { return visitor.VisitClassStmt(this); }
        }
    }  
}