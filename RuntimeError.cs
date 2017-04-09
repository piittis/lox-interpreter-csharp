using System;
using System.Collections.Generic;
using System.Text;

namespace Lox
{
    class RuntimeError : Exception
    {
        public readonly Token token;

        public RuntimeError(Token token, string message) : base(message)
        {
            this.token = token;
        }
    }
}
