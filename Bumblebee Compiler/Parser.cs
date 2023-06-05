using Bumblebee_Compiler.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler {

    internal class ASTNode {
        internal string Type;
        internal List<ASTNode> Body; // TODO bad name
        internal string Value;

        internal ASTNode(string type) {
            this.Type = type;
            Body = new();
            Value = "";
        }

        internal ASTNode(string type, string value) {
            this.Type = type;
            this.Body = new();
            this.Value = value;
        }
    }

    internal class Parser {

        internal ASTNode AST; // Abstract Syntax Tree

        internal Parser() {
            AST = new ASTNode("Program");
        }

        internal void StaticAnalyze(IEnumerable<Token> tokens) {
            IEnumerator<Token> iter = tokens.GetEnumerator();
            if (!iter.MoveNext()) {
                return;
            }
            walk(iter, AST);
        }

        private void walk(IEnumerator<Token> iter, ASTNode parent) {
            if (iter.Current.Type == TokenType.Number) {
                parent.Body.Add(new ASTNode("NumberLiteral", iter.Current.Value));
            } else if (iter.Current.Type == TokenType.Parenth) {
                // TODO
                throw new Exception($"Unexpected symbol \"{iter.Current.Value}\"");
                /*if (token.Value == "(") {

                } else if (token.Value == ")") {

                } else {
                    throw new Exception("Invalid token state for \"Parenth\".");
                }*/
            } else if (iter.Current.Type == TokenType.Literal) {
                // Function or variable, which one is it?
                // (just a function right now)
                ASTNode expression = new ASTNode("CallExpression", iter.Current.Value);

                // Ensure next token is correct
                if (!iter.MoveNext()) throw new Exception("Unexpected EOF. Expected \"(\" for function call.");
                if (iter.Current.Type != TokenType.Parenth || iter.Current.Value != "(") throw new Exception("Unexpected symbol. Expected \"(\" for function call.");

                // Parse parameters
                if (!iter.MoveNext()) throw new Exception("Unexpected EOF. Expected \")\" for end of function call.");
                while (iter.Current.Type != TokenType.Parenth || iter.Current.Value != ")") {
                    walk(iter, expression);
                    if (!iter.MoveNext()) throw new Exception("Unexpected EOF. Expected \")\" for end of function call.");
                }

                // Ensure end token is correct
                if (iter.Current.Type != TokenType.Parenth || iter.Current.Value != ")") throw new Exception("Unexpected symbol. Expected \")\" for end of function call.");

                // Add expression to tree
                parent.Body.Add(expression);
            } else if (iter.Current.Type == TokenType.Operator) {
                // Requires a number before and after
                ASTNode prevNumber = parent.Body.Last();
                if (prevNumber.Type != "NumberLiteral") throw new Exception("Expected a number.");

                string operatorType = iter.Current.Value;

                // Walk to get the next token
                if (!iter.MoveNext()) throw new Exception("Unexpected EOF. Expected number.");
                walk(iter, prevNumber);
                if (prevNumber.Body[0].Type != "NumberLiteral") throw new Exception("Expected a number.");

                // Change previous number into an expression
                prevNumber.Body.Insert(0, new ASTNode("NumberLiteral", prevNumber.Value));
                prevNumber.Type = "Operation";
                prevNumber.Value = operatorType;
            } else {
                throw new Exception($"Unknown token: \"{iter.Current.Type}\":\"{iter.Current.Value}\"");
            }
        }

    }
}
