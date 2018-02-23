using System;
using System.Collections.Generic;
using System.Linq;
using static Lox.TokenType;

namespace Lox
{

    /// <summary>
    /// Recursive descent parser. See engineering Compilers page 119 for implementation tips for LR(1) aka shift reduce parser.
    /// </summary>
    class Parser
    {
        // As the parsing is recursive, to get out of error situations we need to unwind
        // stack to get the parser to a state where it can continue parsing again.
        // To unwind the stack, this exception is thrown and handled at the correct place.
        private class ParseError : Exception { }

        private static readonly TokenType[] BinaryOperators = new TokenType[]
        {
            BANG_EQUAL, EQUAL_EQUAL, GREATER, GREATER_EQUAL, LESS, LESS_EQUAL, PLUS, SLASH, STAR
        };


        private enum FunctionKind
        {
            FUNCTION = 0,
            METHOD = 1,
            GETTER = 2
        }
        private static readonly Dictionary<FunctionKind, string> FunctionKindNames = new Dictionary<FunctionKind, string>()
        {
            { FunctionKind.FUNCTION, "function" },
            { FunctionKind.METHOD, "method" },
            { FunctionKind.GETTER, "getter" }
        };

        private readonly List<Token> tokens;
        private int current = 0;
        private Token CurrentToken => tokens[current];

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }
        
        /// <summary>
        /// Parses the tokens as a list of statements
        /// </summary>
        public List<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        /// <summary>
        /// Parses the tokens as a single expression
        /// </summary>
        public Expr ParseExpression()
        {
            try
            {
                return Expression();
            }
            catch(ParseError)
            {
                return null;
            }
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(CLASS)) return ClassDeclaration();
                if (Match(FUN)) return Function(FunctionKind.FUNCTION);
                if (Match(VAR)) return VarDeclaration();
                return Statement();
            } catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt ClassDeclaration()
        {
            Token name = Consume(IDENTIFIER, "Expect class name.");
            Consume(LEFT_BRACE, "Expect '{' before class body.");

            List<Stmt.Function> methods = new List<Stmt.Function>();
            while (!IsCurrentTokenType(RIGHT_BRACE) && !IsAtEnd())
            {
                methods.Add(Function(FunctionKind.METHOD));
            }

            Consume(RIGHT_BRACE, "Expect '}' afater class body.");

            return new Stmt.Class(name, methods);
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
            if (Match(FOR)) return ForStatement();
            if (Match(IF)) return IfStatement();
            if (Match(PRINT)) return PrintStatement();
            if (Match(RETURN)) return ReturnStatement();
            if (Match(WHILE)) return WhileStatement();
            if (Match(LEFT_BRACE)) return new Stmt.Block(Block());
            return ExpressionStatement();
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private Stmt.Function Function(FunctionKind kind)
        {
            // If method name is preceded with "class", it can be called as a static method/getter.
            bool isStaticMethod = (kind == FunctionKind.METHOD && Match(CLASS));

            Token name = Consume(IDENTIFIER, $"Expect {FunctionKindNames[kind]} name.");

            // try to match a getter.
            if (kind == FunctionKind.METHOD && Match(LEFT_BRACE))
            {
                kind = FunctionKind.GETTER;
                List<Stmt>  getterBody = Block();
                return new Stmt.Function(name, new List<Token>(), getterBody, isStaticMethod, isGetter: true);
            }

            Consume(LEFT_PAREN, $"Expect '(' after {FunctionKindNames[kind]} name.");

            var parameters = new List<Token>();
            if (!IsCurrentTokenType(RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 8)
                    {
                        Error(Peek(), "Cannot have more than 8 parameters.");
                    }
                    parameters.Add(Consume(IDENTIFIER, "Expect parameter name."));
                } while (Match(COMMA));
            }

            Consume(RIGHT_PAREN, "Expect ')' after parameters.");
            Consume(LEFT_BRACE, $"Expect '{{' before {FunctionKindNames[kind]} body.");
            List<Stmt> body = Block();

            return new Stmt.Function(name, parameters, body, isStaticMethod, isGetter: false);
        }

        private Stmt ForStatement()
        {
            Consume(LEFT_PAREN, "Expect '(' after 'for'.");

            // For loop doesn't get its own statement.
            // Instead we desugar and reduce it to a while loop.

            Stmt initializer;
            if (Match(SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(VAR))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr condition = null;
            if (!IsCurrentTokenType(SEMICOLON))
            {
                condition = Expression();
            }
            Consume(SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!IsCurrentTokenType(RIGHT_PAREN))
            {
                increment = Expression();
            }
            Consume(RIGHT_PAREN, "Expect ')' after for clauses.");

            // Parse the statement body of the for loop.
            Stmt body = Statement();
            if (increment != null)
            {
                // Add increment to be executed after the main body.
                body = new Stmt.Block(new List<Stmt>
                {
                    body,
                    new Stmt.Expression(increment)
                });
            }

            if (condition == null)
            {
                condition = new Expr.Literal(true);
            }
            
            body = new Stmt.While(condition, body);
            if (initializer != null)
            {
                // Run the initializer once before the while loop.
                body = new Stmt.Block(new List<Stmt> { initializer, body });
            }

            return body;
        }

        private Stmt IfStatement()
        {
            Consume(LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after if condition.");
            Stmt thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(ELSE))
            {
                elseBranch = Statement();
            }
            return new Stmt.If(condition, thenBranch, elseBranch);

            throw new NotImplementedException();
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(SEMICOLON, "Expect ';' after value.");
            return new Stmt.Print(value);
        }

        private Stmt ReturnStatement()
        {
            Token keyword = Previous();
            Expr value = null;
            if (!IsCurrentTokenType(SEMICOLON))
            {
                value = Expression();
            }

            Consume(SEMICOLON, "Expect ';' after return value.");
            return new Stmt.Return(keyword, value);
        }

        private Stmt WhileStatement()
        {
            Consume(LEFT_PAREN, "Expect '(' after 'while'.");
            Expr condition = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after condition.");
            Stmt body = Statement();

            return new Stmt.While(condition, body);
        }

        private List<Stmt> Block()
        {
            var statements = new List<Stmt>();
            while (!IsCurrentTokenType(RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }
            Consume(RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Expr Expression()
        {
            Expr expr = Assignment();

            while(Match(COMMA))
            {
                Expr right = Assignment();
                expr = new Expr.Comma(expr, right);
            }

            return expr;
        }

        private Expr Assignment()
        {         
            Expr expr = Or();

            // if "=" is next, we are trying to assign a value to the parsed expression.
            // Expression needs to be converted from rvalue to lvalue.
            if (Match(EQUAL))
            {
                Token equals = Previous();
                Expr value = Assignment();
                // Assign to a variable.
                if (expr is Expr.Variable variable)
                {
                    Token name = variable.name;
                    return new Expr.Assign(name, value);
                }
                // Assign to a property.
                else if (expr is Expr.Get get)
                {
                    return new Expr.Set(get.obj, get.name, value);
                }
                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Or()
        {
            Expr expr = And();
            while(Match(OR))
            {
                Token op = Previous();
                Expr right = And();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr And()
        {
            Expr expr = BinaryError();
            while (Match(AND))
            {
                Token op = Previous();
                Expr right = BinaryError();          
                expr = new Expr.Logical(expr, op, right);
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
                Ternary();
            }

            return Ternary();         
        }

        private Expr Ternary()
        {
            Expr expr = Equality();

            if (Match(QUESTION_MARK))
            {
                Expr condition = expr;
                Expr ifTrue = Assignment();
                Consume(COLON, "Expecting ':'");         
                Expr ifFalse = Assignment();
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

            return Call();
        }

        private Expr Call()
        {
            Expr expr = Primary();

            while(true)
            {
                if (Match(LEFT_PAREN))
                {
                    // Trying to call previous expr as a function.
                    expr = FinishCall(expr);
                }
                else if (Match(DOT))
                {
                    Token name = Consume(IDENTIFIER, "expect property name after '.'.");
                    expr = new Expr.Get(expr, name);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            var arguments = new List<Expr>();
            if (!IsCurrentTokenType(RIGHT_PAREN))
            {
                // Parse possible arguments
                do
                {
                    if (arguments.Count >= 8)
                    {
                        Error(Peek(), "Cannot have more than 8 arguments.");
                    }
                    arguments.Add(Assignment());
                }
                while (Match(COMMA));
            }

            // Tokens location is used when reporting errors caused by a function call.
            Token paren = Consume(RIGHT_PAREN, "Expect ')' after arguments.");
            return new Expr.Call(callee, paren, arguments);
        }

        private Expr Primary()
        {

            if (Match(out Token matchedToken, NUMBER, STRING))
                return new Expr.Literal(matchedToken.literal);

            if (Match(FALSE)) return new Expr.Literal(false);
            if (Match(TRUE)) return new Expr.Literal(true);
            if (Match(NIL)) return new Expr.Literal(null);

            if (Match(THIS)) return new Expr.This(Previous());

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
            if (types.Any(IsCurrentTokenType))
            {
                matchedToken = Peek();
                Advance();
                return true;
            }
            matchedToken = null;
            return false;
        }

        private bool IsCurrentTokenType(TokenType tokenType)
        {
            if (IsAtEnd()) return false;
            return Peek().type == tokenType;
        }

        private bool IsAtEnd() => Peek().type == EOF;

        private Token Peek() => tokens[current];

        private Token Advance()
        {
            if (!IsAtEnd()) current++;
            return Previous();
        }

        private Token Previous() => tokens[current - 1];

    }
}