using System;
using System.Collections.Generic;
using System.Linq;
using static Lox.TokenType;

namespace Lox
{


    /// <summary>
    /// Recursive descent parser
    /// </summary>
    class ParserRD
    {
        // As the parsing is recursive, to get out of error situations we need to unwind
        // stack to get the parser to a state where it can continue parsing again.
        // To unwind the stack, this exception is thrown and handled at the correct place.
        private class ParseError : Exception { }

        private static readonly TokenType[] BinaryOperators = new TokenType[]
        {
            BANG_EQUAL, EQUAL_EQUAL, GREATER, GREATER_EQUAL, LESS, LESS_EQUAL, PLUS, SLASH, STAR
        };

        private readonly List<Token> tokens;
        private int current = 0;

        public ParserRD(List<Token> tokens)
        {
            this.tokens = tokens;
        }
        
        public List<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(VAR))
                {
                    return VarDeclaration();
                }

                return Statement();
            } catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt VarDeclaration()
        {
            Token name = Consume(IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if (Match(EQUAL))
            {
                initializer = Expression();
            }

            Consume(SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        private Stmt Statement()
        {
            if (Match(PRINT)) return PrintStatement();
            if (Match(LEFT_BRACE)) return Block();
            return ExpressionStatement();
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(SEMICOLON, "Expect ';' after value.");
            return new Stmt.Print(value);
        }

        private Stmt Block()
        {
            var statements = new List<Stmt>();
            while (!IsCurrentTokenType(RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }
            Consume(RIGHT_BRACE, "Expect '}' after block.");
            return new Stmt.Block(statements);
        }

        private Expr Expression()
        {
            return BinaryError();
        }

        private Expr Assignment()
        {
            Expr expr = BinaryError();

            if (Match(EQUAL))
            {
                Token equals = Previous();
                Expr value = Assignment();
                // Convert from rvalue to lvalue.
                if (expr is Expr.Variable variable)
                {
                    Token name = variable.name;
                    return new Expr.Assign(name, value);
                }
                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr BinaryError()
        {
            if (Match(out Token matchedToken, BinaryOperators))
            {
                // Binary operator should not appear at start of expression.
                Error(matchedToken, "Binary operator without left-hand operand.");
                // Parse possible right hand operand but discard it.
                Comma();
            }

            return Comma();         
        }

        private Expr Comma()
        {
            Expr expr = Ternary();

            while (Match(COMMA))
            {
                Expr right = Ternary();
                expr = new Expr.Comma(expr, right);
            }

            return expr;
        }

        private Expr Ternary()
        {
            Expr expr = Equality();

            if (Match(QUESTION_MARK))
            {
                Expr condition = expr;
                Expr ifTrue = Expression();
                Consume(COLON, "Expecting ':'");         
                Expr ifFalse = Expression();
                expr = new Expr.Ternary(condition, ifTrue, ifFalse);               
            }

            return expr;
        }

        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(out Token op, BANG_EQUAL, EQUAL_EQUAL))
            {
                Expr right = Comparison();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;           
        }

        private Expr Comparison()
        {
            Expr expr = Term();

            while (Match(out Token op, GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
            {
                Expr right = Term();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Term()
        {
            Expr expr = Factor();

            while (Match(out Token op, MINUS, PLUS))
            {
                Expr right = Factor();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Factor()
        {
            Expr expr = Unary();

            while (Match(out Token op, SLASH, STAR))
            {
                Expr right = Unary();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(out Token op, BANG, MINUS))
            {
                Expr right = Unary();
                Expr expr = new Expr.Unary(op, right);
                return expr;
            }

            return Primary();
        }

        private Expr Primary()
        {

            if (Match(out Token matchedToken, NUMBER, STRING))
                return new Expr.Literal(matchedToken.literal);

            if (Match(FALSE)) return new Expr.Literal(false);
            if (Match(TRUE)) return new Expr.Literal(true);
            if (Match(NIL)) return new Expr.Literal(null);

            if (Match(IDENTIFIER)) return new Expr.Variable(Previous());

            if (Match(LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            // can't descent any further
            throw Error(Peek(), "Expect expression.");
        }

        /// <summary>
        /// Discards tokens until a beginning of a statement is found.
        /// Aka panic mode.
        /// Used to synchronize the parser after catching ParseError.
        /// </summary>
        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().type == SEMICOLON) return;

                switch (Peek().type)
                {
                    case CLASS:
                    case FUN:
                    case VAR:
                    case FOR:
                    case IF:
                    case WHILE:
                    case PRINT:
                    case RETURN:
                        return;
                }

                Advance();
            }
        }

        /// <summary>
        /// Consume a token of given type, throw ParseError if another token is found
        /// </summary>
        private Token Consume(TokenType type, string message)
        {
            if (IsCurrentTokenType(type)) return Advance();
            throw Error(Peek(), message);
        }

        private ParseError Error(Token token, String message)
        {
            // report the error to user
            Lox.Error(token, message);
            // return an exception instance, it's upto the caller to throw it or not
            return new ParseError();
        }

        private bool Match(params TokenType[] types)
        {
            if (types.Any(IsCurrentTokenType))
            {
                Advance();
                return true;
            }
            return false;
        }

        private bool Match(out Token matchedToken, params TokenType[] types)
        {
            foreach(TokenType type in types)
            {
                if (IsCurrentTokenType(type))
                {
                    matchedToken = Peek();
                    Advance();
                    return true;
                }
            }
            matchedToken = null;
            return false;
        }

        private bool IsCurrentTokenType(TokenType tokenType)
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