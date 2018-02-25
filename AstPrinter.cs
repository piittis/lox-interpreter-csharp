using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lox
{
    /// <summary>
    /// generates a string representation for different expressions
    /// </summary>
    class AstPrinter : Expr.IVisitor<string>, Stmt.IVisitor<string>
    {

        public string Print(List<Stmt> statements)
        {
            return string.Join(System.Environment.NewLine, statements.Select(Print));         
        }

        public string Print(Stmt stmt)
        {
            return stmt.Accept(this);
        }

        public string VisitAssignExpr(Expr.Assign expr)
        {
            return $"({expr.name} <- {expr.value.Accept(this)})";
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.left, expr.right);
        }

        public string VisitBlockStmt(Stmt.Block stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitCallExpr(Expr.Call expr)
        {
            return $"({expr.callee.Accept(this)}()";
        }

        public string VisitClassStmt(Stmt.Class stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitCommaExpr(Expr.Comma expr)
        {
            return Parenthesize("comma", expr.left, expr.right);
        }

        public string VisitExpressionStmt(Stmt.Expression stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitFunctionStmt(Stmt.Function stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitGetExpr(Expr.Get expr)
        {
            return $"(value of {expr.Accept(this)})";
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.expression);
        }

        public string VisitIfStmt(Stmt.If stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value.ToString();
        }

        public string VisitLogicalExpr(Expr.Logical expr)
        {
            return Parenthesize(expr.op.lexeme, expr.left, expr.right);
        }

        public string VisitPrintStmt(Stmt.Print stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitReturnStmt(Stmt.Return stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitSetExpr(Expr.Set expr)
        {
            return $"({expr.obj.Accept(this)}{expr.name} <- {expr.value.Accept(this)})";
        }

        public string VisitSuperExpr(Expr.Super expr)
        {
            throw new NotImplementedException();
        }

        public string VisitTernaryExpr(Expr.Ternary expr)
        {
            return $"({expr.condition.Accept(this)} ? {expr.ifTrue.Accept(this)} : {expr.ifFalse.Accept(this)})";
        }

        public string VisitThisExpr(Expr.This expr)
        {
            throw new NotImplementedException();
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.right);
        }

        public string VisitVariableExpr(Expr.Variable expr)
        {
            return $"({expr.name})";
        }

        public string VisitVarStmt(Stmt.Var stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitWhileStmt(Stmt.While stmt)
        {
            throw new NotImplementedException();
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
