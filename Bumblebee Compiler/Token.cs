using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler
{
    internal enum TokenType {
        EndOfFile = -1,
        OpenParenthesis,
        CloseParenthesis,
        OpenBracket,
        CloseBracket,
        Identifier,
        //Whitespace,
        Comment,
        ArgumentSeparator,
        NumberLiteral,
        BoolLiteral,
        CharLiteral,
        Equals,
        NotEquals,
        And,
        Or,
        Not,
        Const,
        LineDelimiter,
    }

    internal abstract class Token
    {
        internal readonly TokenType Type;
        internal abstract object Value { get; }

        protected Token(TokenType type)
        {
            Type = type;
        }

        public override string ToString()
        {
            object value = this.Value;
            if (value != null) {
                return this.Type.ToString() + "[" + value.ToString() + "]";
            } else {
                return Type.ToString();
            }
        }
    }

    internal abstract class LiteralToken : Token {
        private readonly object Constant;
        internal override object Value => Constant;

        protected LiteralToken(TokenType type, object constant) : base(type) {
            this.Constant = constant;
        }
    }

    internal class CharLiteralToken : Token {

    }

}
