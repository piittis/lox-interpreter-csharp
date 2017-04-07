using System;
using System.Collections.Generic;
using System.Linq;
using static Lox.TokenType;

namespace Lox
{
    /// <summary>
    /// recursive descent parser
    /// </summary>
    /// 
  
    /*  GRAMMAR:
     *
     *  expression → equality
     *  equality   → comparison ( ( "!=" | "==" ) comparison )*      
     *  comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )*
     *  term       → factor ( ( "-" | "+" ) factor )*
     *  factor     → unary ( ( "/" | "*" ) unary )*
     *  unary      → ( "!" | "-" ) unary
     *              | primary
     *  primary    → NUMBER | STRING | "false" | "true" | "nil"
     *              | "(" expression ")"      
     */

    class Parser
    {
        private readonly List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        private Expr Expression()
        {
            return Equality();
        }

        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(BANG_EQUAL, EQUAL_EQUAL))
            {
                Token op = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
            
        }

        private Expr Comparison()
        {
            Expr expr = Term();

            while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
            {
                Token op = Previous();
                Expr right = Term();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Term()
        {

            Expr expr = Factor();

            //todo use out?
            while (Match(SLASH, STAR))
            {
                Token op = Previous();
                Expr right = Factor();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Factor()
        {
            Expr expr = Unary();
            while (Match(SLASH, STAR))
            {
                Token op = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(BANG, MINUS))
            {
                Token op = Previous();
                Expr right = Unary();
                Expr expr = new Expr.Unary(op, right);
                return expr;
            }
            return Primary();
        }

        private Expr Primary()
        {
            if (Match(FALSE)) return new Expr.Literal(false);
            if (Match(TRUE)) return new Expr.Literal(true);
            if (Match(FALSE)) return new Expr.Literal(null);

            if (Match(NUMBER, STRING))
            {
                return new Expr.Literal(Previous().literal);
            }

            if (Match(FALSE)) return new Expr.Literal(false)            
            if (Match(LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }
        }

        private void Consume(TokenType rIGHT_PAREN, string v)
        {
            throw new NotImplementedException();
        }

        private bool Match(params TokenType[] types)
        {
            //todo decide implementation
            if (types.Contains(Peek().type))
            {

            }
            if (types.Any(Check))
            {
                Advance();
                return true;
            }
            // or foreach...

            return false;
        }

        //todo utilize properties for these and order

        //todo name? IsCurrentType or something?
        private bool Check(TokenType tokenType)
        {
            if (IsAtEnd()) return false;
            return Peek().type == tokenType;
        }
        private bool IsAtEnd()
        {
            return Peek().type == EOF;
        }

        private Token Peek()
        {
            return tokens[current];
        }
        private Token Advance()
        {
            if (!IsAtEnd()) current++;
            return Previous();
        }

        private Token Previous()
        {
            return tokens[current - 1];
        }

    }
}
