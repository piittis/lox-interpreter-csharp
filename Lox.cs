using System;
using System.Collections.Generic;

namespace Lox
{
    class Lox
    {
        private static readonly Interpreter interpreter = new Interpreter();
        private static bool hadError = false;
        private static bool hadRuntimeError = false;

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

            if (hadError) System.Environment.Exit(65);
            if (hadRuntimeError) System.Environment.Exit(70);
        }

        private static void RunPrompt()
        {
            while (true)
            {
                // run a REPL
                Console.Write("> ");
                string line = Console.ReadLine();
                if (line != null)
                {
                    Run(line);
                    hadError = false;
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
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void Error(Token token, String message)
        {
            if (token.type == TokenType.EOF)
            {
                Report(token.line, " at end", message);
            }
            else
            {
                Report(token.line, $" at '{token.lexeme}'", message);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.Error.WriteLine($"{error.Message} \n[line {error.token.line}]");
            hadRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
            hadError = true;
        }
    }
}
