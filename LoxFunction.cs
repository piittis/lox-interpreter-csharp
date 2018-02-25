using System;
using System.Collections.Generic;

namespace Lox
{
    class LoxFunction : ICallable
    {
        private readonly Stmt.Function declaration;
        // Closure captures the environment around the function.
        private readonly Environment closure;
        private readonly bool isInitializer;

        // C# getter that tells if this LoxFunction is also a getter. When getter is "got", we execute its body and return the result.
        public bool IsGetter => declaration.isGetter;

        public LoxFunction(Stmt.Function declaration, Environment closure, bool isInitializer)
        {
            this.declaration = declaration;
            this.closure = closure;
            this.isInitializer = isInitializer;
        }

        public LoxFunction Bind(LoxInstance instance, IClass superClass)
        {
            var environment = new Environment(closure);
            environment.Define("this", instance);
            environment.Define("super", superClass);
            return new LoxFunction(declaration, environment, isInitializer);
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

            // If init() is called directly, return the class instance
            if (isInitializer) return closure.GetAt(0, declaration.name);

            return null;
        }

        public override string ToString() => $"<fn {declaration.name.lexeme}>";

    }
}
