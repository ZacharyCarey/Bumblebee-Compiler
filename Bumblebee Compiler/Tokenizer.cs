using Bumblebee_Compiler.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bumblebee_Compiler {
    internal class Tokenizer {

        static RegexOptions options = RegexOptions.Multiline;
        //static Regex LETTERS = new Regex(@"[a-zA-Z]", options);
        static Regex WHITESPACE = new Regex(@"\s+", options);
        static Regex NUMBERS = new Regex(@"\d+", options);
        static Regex COMMENT = new Regex(@"\/\/(.*)[\r\n]*$", options);
        static Regex IDENTIFIER = new Regex(@"[_a-zA-Z]\w*", options);
        static Regex OPERATOR = new Regex(@"[+\-*\/=!%]|and|or|xor|>>|<<|>|<", options);

        internal Tokenizer() {
        }

        internal IEnumerable<Token> ParseTokens(string input) {
            /*            Token token = new();
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
                        }*/

            int current = 0;
            while (current < input.Length) {
                char c = input[current];
                Match match;

                if (c == '(' || c == ')' || c == '{' || c == '}') {
                    yield return new Token(TokenType.Paren, c.ToString());
                    current++;
                    continue;
                }

                if (c == ';') {
                    yield return new Token(TokenType.LineDelimiter, ";");
                    current++;
                    continue;
                }

                match = COMMENT.Match(input, current);
                if (match.Success && match.Index == current) {
                    yield return new Token(TokenType.Comment, match.Groups[1].Value);
                    current += match.Length;
                    continue;
                }

                match = IDENTIFIER.Match(input, current);
                if (match.Success && match.Index == current) {
                    yield return new Token(TokenType.Identifier, match.Value);
                    current += match.Length;
                    continue;
                }

                match = WHITESPACE.Match(input, current);
                if (match.Success && match.Index == current) {
                    //yield return new Token(TokenType.Whitespace, match.Value);
                    current += match.Length;
                    continue;
                }

                match = NUMBERS.Match(input, current);
                if (match.Success && match.Index == current) {
                    yield return new Token(TokenType.Number, match.Value);
                    current += match.Length;
                    continue;
                }

                match = OPERATOR.Match(input, current);
                if (match.Success && match.Index == current) {
                    yield return new Token(TokenType.Operator, match.Value);
                    current += match.Length;
                    continue;
                }

                throw new Exception($"Unknown char: '{c}'");
            }
        }

    }
}
