using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler.Tokens {
    internal abstract class Token {

        abstract internal void Parse(string input);

    }
}
