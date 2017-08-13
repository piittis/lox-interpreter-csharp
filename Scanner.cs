using System;
using System.Collections.Generic;
using System.Globalization;
using static Lox.TokenType;

namespace Lox
{
    class Scanner
    {
        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();

        private int start = 0;
        private int current = 0;
        private int line = 1;

        private char CurrentChar => source[current];

        private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
        {
            { "and",    AND },
            { "class",  CLASS },
            { "else",   ELSE },
            { "false",  FALSE },
            { "for",    FOR },
            { "fun",    FUN },
            { "if",     IF },
            { "nil",    NIL },
            { "or",     OR },
            { "print",  PRINT },
            { "return", RETURN },
            { "super",  SUPER },
            { "this",   THIS },
            { "true",   TRUE },
            { "var",    VAR },
            { "while",  WHILE }
        };

        public Scanner(string source)
        {
            this.source = source;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                // We are at the beginning of the next lexeme.
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(EOF, "", null, line));
            return tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(LEFT_PAREN); break;
                case ')': AddToken(RIGHT_PAREN); break;
                case '{': AddToken(LEFT_BRACE); break;
                case '}': AddToken(RIGHT_BRACE); break;
                case ',': AddToken(COMMA); break;
                case '.': AddToken(DOT); break;
                case '-': AddToken(MINUS); break;
                case '+': AddToken(PLUS); break;
                case ';': AddToken(SEMICOLON); break;
                case '*': AddToken(STAR); break;
                case ':': AddToken(COLON); break;
                case '?': AddToken(QUESTION_MARK); break;
                case '!': AddToken(Match('=') ? BANG_EQUAL : BANG); break;
                case '=': AddToken(Match('=') ? EQUAL_EQUAL : EQUAL); break;
                case '<': AddToken(Match('=') ? LESS_EQUAL : LESS); break;
                case '>': AddToken(Match('=') ? GREATER_EQUAL : GREATER); break;
                case '/':
                    if (Match('/'))
                    {
                        // eat the whole line
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    }
                    else if (Match('*'))
                    {
                        ParseBlockComment();
                    }
                    else
                    {
                        AddToken(SLASH);
                    }
                    break;

                case ' ':
                case '\r':
                case '\t':
                    // ignore whitespace
                    break;

                case '\n':
                    line++;
                    break;

                case '"': ParseString(); break;

                default:
                    if (IsDigit(c))
                    {
                        ParseNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        ParseIdentifier();
                    }
                    else
                    {
                        Lox.Error(line, "Unexpected character.");
                    } 
                    break;
            }
        }

        #region parsing

        private void ParseBlockComment()
        {
            bool closed = false;
            while(!IsAtEnd())
            {
                if (Peek() == '\n')
                {
                    line++;
                }
                else if (Peek() == '*' && PeekNext() == '/')
                {
                    // eat the */
                    Advance();
                    Advance();
                    closed = true;
                    break;
                }

                Advance();                         
            }
            
            if (!closed)
            {
                Lox.Error(line, "Unclosed block comment.");
            }
        }

        private void ParseIdentifier()
        {
            while (IsAlphaNumeric(Peek()))
            {
                Advance();
            }
            string text = source.Substring(start, current - start);

            if (!keywords.TryGetValue(text, out TokenType token))
            {
                token = IDENTIFIER;
            }

            AddToken(token);
        }

        private void ParseNumber()
        {
            while (IsDigit(Peek()))
            {
                Advance();
            }

            // look for fractional part
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();

                while (IsDigit(Peek()))
                {
                    Advance();
                }
            }

            var value = source.Substring(start, current - start);
            AddToken(NUMBER, Double.Parse(value, CultureInfo.InvariantCulture));
        }

        private void ParseString()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n')
                {
                    line++;
                }
                Advance();
            }

            if (IsAtEnd())
            {
                Lox.Error(line, "Unterminated string.");
                return;
            }

            // the closing "
            Advance();

            // trim the surrounding quotes
            string value = source.Substring(start + 1, current - start - 2);
            AddToken(STRING, value);
        }

        #endregion

        private char Peek()
        {
            if (current >= source.Length) return '\0';
            return source[current];
        }

        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        private char Advance()
        {
            current++;
            return source[current - 1];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, Object literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }

        #region helpers

        private bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';

        private bool IsDigit(char c) => c >= '0' && c <= '9';

        private bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

        private bool IsAtEnd() => current >= source.Length;

        #endregion
        
    }
}
