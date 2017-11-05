using System;
using System.Collections.Generic;

namespace Lox
{
    class LoxFunction : ICallable
    {
        private readonly Stmt.Function declaration;
        // Closure captures the environment around the function.
        private readonly Environment closure;

        public LoxFunction(Stmt.Function declaration, Environment closure)
        {
            this.declaration = declaration;
            this.closure = closure;
        }

        public LoxFunction Bind(LoxInstance instance)
        {
            var environment = new Environment(closure);
            environment.Define("this", instance);
            return new LoxFunction(declaration, environment);
        }

        public int Arity() => declaration.parameters.Count;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(closure);

            for (int i = 0; i < declaration.parameters.Count; i++)
            {
                environment.Define(declaration.parameters[i].lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(declaration.body, environment);
            }
            catch (Return retVal)
            {
                return retVal.value;
            }
            return null;
        }

        public override string ToString() => $"<fn {declaration.name.lexeme}>";

    }
}
