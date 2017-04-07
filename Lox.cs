using System;
using System.Collections.Generic;

namespace Lox
{
    class Lox
    {
        private static bool hadError;

        static void Main(string [] args)
        {


           /* var expr = new Binary(new Unary(new Token(TokenType.MINUS, "-", null, 1), new Literal(123)),
                                  new Token(TokenType.STAR, "*", null, 1),
                                  new Grouping(new Literal(45.67)));

            Console.WriteLine(new AstPrinterRpn().Print(expr));*/

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

            if (hadError)
            {
                Environment.Exit(65);
            }
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

            Console.WriteLine($"running: {source}");

            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
            hadError = true;
        }
    }
}
