using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler.Tokens {
    internal class NumberToken : Token {

        internal int Value;

        internal override void Parse(string input) {
            Value = int.Parse(input);
        }
    }
}
