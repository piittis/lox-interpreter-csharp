using System.Collections.Generic;

namespace Lox
{
    /// <summary>
    /// Represents an instance of a class
    /// </summary>
    class LoxInstance
    {
        private LoxClass klass;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        // Expose properties via an indexer.
        public object this[Token name]
        {
            get
            {
                if (fields.TryGetValue(name.lexeme, out object val))
                    return val;

                var method = klass.FindMethod(this, name.lexeme);
                if (method != null) return method;

                throw new RuntimeError(name, $"Undefined property '{name.lexeme}'");
            }
            set
            {
                fields.AddOrUpdate(name.lexeme, value);
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