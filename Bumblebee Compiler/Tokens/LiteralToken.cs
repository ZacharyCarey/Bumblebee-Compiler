using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler.Tokens {
    internal class LiteralToken : Token {

        internal string Value;

        internal override void Parse(string input) {
            this.Value = input;
        }
    }
}
