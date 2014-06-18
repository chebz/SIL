using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIL_Core
{
    public class SyntaxErrorException : Exception
    {
        public SyntaxErrorException()
            : base("Syntax Error!")
        {
        }
    }
}
