using System.Collections.Generic;

namespace Lox
{
    class LoxClass : ICallable
    {
        private readonly string name;

        public LoxClass(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            LoxInstance instance = new LoxInstance(this);
            return instance;
        }

        public int Arity()
        {
            return 0;
        }
    }
}