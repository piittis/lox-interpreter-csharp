using System.Collections.Generic;

namespace Lox
{
    class LoxInstance
    {
        private LoxClass klass;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        // Expose fields via an indexer.
        public object this[Token name]
        {
            get
            {
                if (fields.TryGetValue(name.lexeme, out object val))
                    return val;

                throw new RuntimeError(name, $"Undefined property '{name.lexeme}'");
            }
            set
            {
                if (fields.ContainsKey(name.lexeme))
                    fields[name.lexeme] = value;
                else
                    fields.Add(name.lexeme, value);
            }
        }

        public LoxInstance(LoxClass klass)
        {
            this.klass = klass;
        }

        public override string ToString()
        {
            return klass.ToString() + " instance";
        }
    }
}