using System;
using System.Collections.Generic;

namespace Lox
{
    class ParserRL
    {

        /* 
         * Might implement this.
         * LR(1) aka shift reduce parser.
         * See engineering compilers page 119 for implementation tips.
         * 
         */

        private readonly List<Token> tokens;

        public ParserRL(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public Expr Parse()
        {
            throw new NotImplementedException();
        }
    }
}
