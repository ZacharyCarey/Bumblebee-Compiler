using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler.Tokens {
    internal struct Token {

        internal TokenType Type;
        internal string Value;

        public Token(TokenType type, string value) {
            this.Type = type;
            this.Value = value;
        }

        public override string ToString() {
            return $"[Type: {Type}, Value: '{Value}']";
        }
    }

    internal enum TokenType {
        Paren,
        Identifier,
        //Whitespace,
        Comment,
        NumberLiteral,
        BoolLiteral,
        CharLiteral,
        LineDelimiter,
        Operator,
        Iteration,
        Selection,
        ArgumentSeparator,
        VariableModifier
    }
}
