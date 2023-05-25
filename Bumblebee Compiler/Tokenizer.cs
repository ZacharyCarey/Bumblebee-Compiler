using Bumblebee_Compiler.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler {
    internal class Tokenizer {

        private StreamReader reader;

        internal Tokenizer(StreamReader inputFile) {
            this.reader = inputFile;
        }

        internal IEnumerable<Token> ParseTokens() {
            int c;
            while ((c = reader.Read()) >= 0) {
                if (char.IsWhiteSpace((char)c)) {
                    // Ignore whitespace between tokens
                    continue;
                } else if (c >= '0' && c <= '9') {
                    yield return ReadNumber(c);
                } else {
                    yield return ReadLiteral(c);
                }
            }
        }

        private Token ReadNumber(int firstChar) {
            string numbers = "" + ((char)firstChar);
            int c;
            while ((c = reader.Read()) >= 0) {
                if (char.IsWhiteSpace((char)c)) {
                    break;
                } else if (c >= '0' && c <= '9') {
                    numbers += (char)c;
                } else {
                    throw new Exception("Invalid number.");
                }
            }

            NumberToken token = new NumberToken();
            token.Parse(numbers);
            return token;
        }

        private Token ReadLiteral(int firstChar) {
            string literal = "" + ((char)firstChar);
            int c;
            while ((c = reader.Read()) >= 0) {
                if (char.IsWhiteSpace((char)c)) {
                    break;
                } else {
                    literal += (char)c;
                }
            }

            LiteralToken token = new LiteralToken();
            token.Parse(literal);
            return token;
        }

    }
}
