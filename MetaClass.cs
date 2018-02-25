using System;
using System.Collections.Generic;
using System.Text;

namespace Lox
{
    /// <summary>
    /// Classes are instances of metaclasses. Metaclass contains static methods for a class.
    /// </summary>
    class MetaClass : IClass
    {
        private readonly string name;
        private readonly IClass superclass;
        private readonly Dictionary<string, LoxFunction> methods;

        public MetaClass(string name, IClass superclass, Dictionary<string, LoxFunction> methods)
        {
            this.name = name;
            this.superclass = superclass;
            this.methods = methods;
        }

        public LoxFunction FindMethod(LoxInstance instance, string name)
        {
            if (methods.TryGetValue(name, out LoxFunction fun))
            {
                // Methods "this" is set to a given instance (from which instance it is accessed from).
                return fun.Bind(instance, superclass);
            }
            if (superclass != null)
            {
                return superclass.FindMethod(instance, name);
            }

            return null;
        }

    }
}
