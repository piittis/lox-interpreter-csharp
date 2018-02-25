using System.Collections.Generic;

namespace Lox
{
    /// <summary>
    /// Represents an instance of a class or metaclass. Instance contains properties that can be accessed.
    /// If property not found as a field, try to find it from class methods.
    /// </summary>
    class LoxInstance
    {
        private IClass instanceOf;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        public IClass InstanceOf => instanceOf;

        // Expose properties via an indexer.
        public object this[Token name]
        {
            get
            {
                if (fields.TryGetValue(name.lexeme, out object val))
                    return val;

                var method = instanceOf.FindMethod(this, name.lexeme);
                if (method != null)
                    return method;

                throw new RuntimeError(name, $"Undefined property '{name.lexeme}'");
            }
            set
            {
                fields.AddOrUpdate(name.lexeme, value);
            }
        }

        public LoxInstance(IClass klass)
        {
            SetClass(klass);
        }

        public LoxInstance() { }


        public void SetClass(IClass klass)
        {
            instanceOf = klass;
        }

        public override string ToString()
        {
            return instanceOf.ToString() + " instance";
        }
    }
}