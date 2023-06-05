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
            Token token = new();
            bool tokenUsed = false; // Used to keep track of tokens that span multiple characters (like numbers)
            int c;
            while ((c = reader.Read()) >= 0) {
                if (char.IsWhiteSpace((char)c)) {
                    // Spaces indicate end of tokens
                    if (tokenUsed) {
                        yield return token;
                        tokenUsed = false;
                        token = new();
                    }

                    // Ignore whitespace between tokens
                    continue;
                } else if (c >= '0' && c <= '9') {
                    // Check for previvous tokens
                    if (tokenUsed) {
                        if (token.Type == TokenType.Number) {
                            // Continue adding to the previous token
                            token.Value += (char)c;
                            continue;
                        } else {
                            // Reached the end of a token
                            yield return token;
                            // Fall through to create a new one for this token
                        }
                    }

                    // Start a new token
                    tokenUsed = true;
                    token.Type = TokenType.Number;
                    token.Value = "" + (char)c;
                } else if (c == '(' || c == ')') {
                    if (tokenUsed) {
                        yield return token;
                    }
                    token.Type = TokenType.Parenth;
                    token.Value = "" + (char)c;
                    yield return token;

                    tokenUsed = false;
                } else if (c == '+') { 
                    if (tokenUsed) {
                        // Return any previous tokens
                        yield return token;
                    }

                    tokenUsed = false;
                    token.Type = TokenType.Operator;
                    token.Value = "" + (char)c;
                    yield return token;
                } else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) {
                    // Letters detected
                    // Check for previvous tokens
                    if (tokenUsed) {
                        if (token.Type == TokenType.Literal) {
                            // Continue adding to the previous token
                            token.Value += (char)c;
                            continue;
                        } else {
                            // Reached the end of a token
                            yield return token;
                            // Fall through to create a new one for this token
                        }
                    }

                    // Start a new token
                    tokenUsed = true;
                    token.Type = TokenType.Literal;
                    token.Value = "" + (char)c;
                } else {
                    //yield return ReadLiteral(c);
                    throw new Exception($"Unknown char: \"{(char)c}\"");
                }
            }
        }

    }
}
