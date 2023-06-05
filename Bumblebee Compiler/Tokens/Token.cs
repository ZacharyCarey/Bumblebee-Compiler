using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler.Tokens {
    internal struct Token {

        internal TokenType Type;
        internal string Value;

        public Token() {
            this.Type = TokenType.Literal;
            this.Value = "";
        }

        public Token(TokenType type, string value) {
            this.Type = type;
            this.Value = value;
        }

    }

    internal enum TokenType {
        Parenth,
        Number,
        Literal,
        Operator
    }
}
