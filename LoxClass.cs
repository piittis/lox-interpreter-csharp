using System.Collections.Generic;

namespace Lox
{
    class LoxClass : ICallable
    {
        private readonly string name;
        private readonly Dictionary<string, LoxFunction> methods;

        public LoxClass(string name, Dictionary<string, LoxFunction> methods)
        {
            this.name = name;
            this.methods = methods;
        }

        public LoxFunction FindMethod(LoxInstance instance, string name)
        {
            if (methods.TryGetValue(name, out LoxFunction fun))
            {
                // Methods "this" is set to a given instance (from which instance it is accessed from).
                return fun.Bind(instance);
            }

            return null;
        }

        /// <summary>
        /// When a class is called with (), we construct a new instance of it.
        /// </summary>
        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var instance = new LoxInstance(this);
            if (methods.TryGetValue("init", out LoxFunction initializer))
            {
                initializer.Bind(instance).Call(interpreter, arguments);
            }
            return instance;
        }

        /// <summary>
        /// Arity of class is the amount of arguments its init method wants.
        /// </summary>
        public int Arity()
        {
            if (!methods.TryGetValue("init", out LoxFunction initializer))
            {
                return 0;
            }
            return initializer.Arity();
        }

        public override string ToString() => name;

    }
}