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
        static Regex BOOL_LITERAL = new Regex(@"false|true", options);
        static Regex CHAR_LITERAL = new Regex(@"'(.)'|'\\(.)'");
        static Regex COMMENT = new Regex(@"\/\/(.*)[\r\n]*$", options);
        static Regex IDENTIFIER = new Regex(@"[_a-zA-Z]\w*", options);
        static Regex OPERATOR = new Regex(@"and|or|xor|not|>>|<<|<=|>=|>|<|==|!=|[+\-*\/=%&|^~]", options);
        static Regex ITERATION = new Regex(@"while", options);
        static Regex SELECTION = new Regex(@"if|else", options);
        static Regex VARIABLE_MODIFIER = new Regex(@"const", options);
        // TODO at the moment, elseif is accepted. Need regex to check for whitespace separators

        internal Tokenizer() {
        }

        internal IEnumerable<Token> ParseTokens(string input) {
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

                if (c == ',') {
                    yield return new Token(TokenType.ArgumentSeparator, ",");
                    current++;
                    continue;
                }

                match = COMMENT.Match(input, current);
                if (match.Success && match.Index == current) {
                    yield return new Token(TokenType.Comment, match.Groups[1].Value);
                    current += match.Length;
                    continue;
                }

                match = ITERATION.Match(input, current);
                if (match.Success && match.Index == current) {
                    yield return new Token(TokenType.Iteration, match.Value);
                    current += match.Length;
                    continue;
                }

                match = SELECTION.Match(input, current);
                if (match.Success && match.Index == current) {
                    yield return new Token(TokenType.Selection, match.Value);
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
                    yield return new Token(TokenType.NumberLiteral, match.Value);
                    current += match.Length;
                    continue;
                }

                match = BOOL_LITERAL.Match(input, current);
                if (match.Success && match.Index == current) {
                    yield return new Token(TokenType.BoolLiteral, match.Value);
                    current += match.Length;
                    continue;
                }

                match = VARIABLE_MODIFIER.Match(input, current);
                if (match.Success && match.Index == current) {
                    yield return new Token(TokenType.VariableModifier, match.Value);
                    current += match.Length;
                    continue;
                }

                match = CHAR_LITERAL.Match(input, current);
                if (match.Success && match.Index == current) {
                    char ch = match.Value[1];
                    if (ch == '\\') {
                        switch(match.Value[2]) {
                            case '\'': 
                            case '\"':
                            case '\\':
                                ch = match.Value[2];
                                break;
                            case '0': ch = '\0'; break;
                            case 'a': ch = '\a'; break;
                            case 'b': ch = '\b'; break;
                            case 'f': ch = '\f'; break;
                            case 'n': ch = '\n'; break;
                            case 'r': ch = '\r'; break;
                            case 't': ch = '\t'; break;
                            case 'v': ch = '\v'; break;
                            default: throw new Exception("Invalid escape character");
                        }
                    } else if (ch == '\'' || ch == '\"' || ch < ' ' || ch > '~') {
                        throw new Exception("Invalid ASCII character.");
                    }
                    yield return new Token(TokenType.CharLiteral, ch.ToString());
                    current += match.Length;
                    continue;
                }

                match = OPERATOR.Match(input, current);
                if (match.Success && match.Index == current) {
                    yield return new Token(TokenType.Operator, match.Value);
                    current += match.Length;
                    continue;
                }

                match = IDENTIFIER.Match(input, current);
                if (match.Success && match.Index == current) {
                    yield return new Token(TokenType.Identifier, match.Value);
                    current += match.Length;
                    continue;
                }

                throw new Exception($"Unknown char: '{c}'");
            }
        }

    }
}
