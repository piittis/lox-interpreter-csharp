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
                // Methods "this" is set to the instance that is is called from.
                return fun.Bind(instance);
            }

            return null;
        }

        /// <summary>
        /// When a class is called with (), we construct a new instance of it.
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public object Call(Interpreter interpreter, List<object> arguments)
        {
            LoxInstance instance = new LoxInstance(this);
            return instance;
        }

        public int Arity()
        {
            return 0;
        }

        public override string ToString()
        {
            return name;
        }
    }
}