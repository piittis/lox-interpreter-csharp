using System;
using System.Collections.Generic;

namespace Lox
{
    class Lox
    {
        private static readonly Interpreter interpreter = new Interpreter();
        private static bool hadError = false;
        private static bool hadRuntimeError = false;
        private static List<String> Errors = new List<String>();

        static void Main(string [] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jlox [script]");
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunFile(string path)
        {
            string text = System.IO.File.ReadAllText(path);
            Run(text);

            Console.Write("Finished, press any key...");
            Console.ReadKey();

            if (hadError) System.Environment.Exit(65);
            if (hadRuntimeError) System.Environment.Exit(70);
        }

        /// <summary>
        /// Runs a REPL
        /// </summary>
        private static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (line != null)
                {
                    List<Token> tokens = new Scanner(line).ScanTokens();
                    List<Stmt> statements = new ParserRD(tokens).Parse();
                    if (!hadError)
                    {
                        interpreter.Interpret(statements);
                        continue;
                    }

                    // Normal statement parsing failed, try to parse as an expression.
                    hadError = false;
                    Expr expr = new ParserRD(tokens).ParseExpression();
                    if (!hadError)
                    {
                        Console.WriteLine(interpreter.EvaluateExpr(expr));
                        continue;
                    }
                    else
                    {
                        // Both parse attempts failed.
                        ReportErrors();
                        ClearErrors();
                        hadError = false;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private static void Run(string source)
        {
            if (source == null)
            {
                return;
            }

            Console.WriteLine($"running:\n{source}");

            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();

            ParserRD parser = new ParserRD(tokens);
            List<Stmt> statements = parser.Parse();        

            if (!hadError)
            {
                interpreter.Interpret(statements);
            }
            else
            {
                ReportErrors();
            }
        }

        public static void Error(int line, string message)
        {
            AddError(line, "", message);
        }

        public static void Error(Token token, String message)
        {
            if (token.type == TokenType.EOF)
            {
                AddError(token.line, " at end", message);
            }
            else
            {
                AddError(token.line, $" at '{token.lexeme}'", message);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.Error.WriteLine($"{error.Message}\n[line {error.token.line}]");
            hadRuntimeError = true;
        }

        private static void AddError(int line, string where, string message)
        {
            Errors.Add($"[line {line}] Error{where}: {message}");
            hadError = true;
        }

        private static void ReportErrors()
        {
            foreach(var errorString in Errors)
            {
                Console.Error.WriteLine(errorString);
            }
        }

        private static void ClearErrors()
        {
            Errors.Clear();
        }
    }
}
