using System;
using System.Collections.Generic;
using System.Text;

namespace Lox
{
    interface IClass
    {
        LoxFunction FindMethod(LoxInstance instance, string name);
    }
}
