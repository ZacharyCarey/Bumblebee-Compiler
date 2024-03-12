using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler {
    internal class TypeChecker {

        private Dictionary<string, VariableOptions> knownVariables = new();
        private Dictionary<string, FunctionOptions> knownFunctions = new();

        internal void CheckTypes(Parser parser, ICompiler compiler) {
            this.knownVariables = compiler.KnownVariables;
            this.knownFunctions = compiler.KnownFunctions;
            walk(parser.AST);
        }

        string walk(ASTNode root) {
            switch(root.Type) {
                case ASTType.StatementBlock:
                    walkStatementBlock(root);
                    return "void";
                case ASTType.DeclarationStatement:
                    walkDeclarationStatement(root);
                    return "void";
                case ASTType.ExpressionAssignmentStatement:
                    walkExpressionAssignmentStatement(root);
                    return "void";
                case ASTType.ExpressionOperator:
                    return walkExpressionOperator(root);
                case ASTType.ExpressionNumber:
                    return walkExpressionNumber(root);
                case ASTType.ExpressionIdentifier:
                    return walkExpressionIdentifier(root, false, true);
                case ASTType.Comment:
                    return "void";
                default:
                    throw new Exception("Unknown statement.");
            }
        }

        void walkStatementBlock(ASTNode root) {
            if (root.Type != ASTType.StatementBlock) throw new Exception("Expected statement block.");
            foreach (var node in root.Params) {
                walk(node);
            }
        }

        void walkDeclarationStatement(ASTNode root) {
            if (root.Type != ASTType.DeclarationStatement) throw new Exception("Expected declaration statement.");
            string type = root.Value;

            ASTNode identifier = root.Params[0];
            if (identifier.Type != ASTType.ExpressionIdentifier) throw new Exception("Expected identifier.");
            string name = identifier.Value;

            if (knownVariables.ContainsKey(name) || knownFunctions.ContainsKey(name)) {
                throw new Exception($"The identifier '{name}' has been previously declared.");
            }

            VariableOptions options = new();
            options.IsReadable = true;
            options.IsWritable = true;
            options.TypeName = type;
            knownVariables.Add(name, options);

            if (root.Params.Count > 1) {
                if (root.Params[1].Type != ASTType.ExpressionAssignmentStatement) throw new Exception("Expected assignment statement");
                walkExpressionAssignmentStatement(root.Params[1]);
            }
        }

        void walkExpressionAssignmentStatement(ASTNode root) {
            if (root.Type != ASTType.ExpressionAssignmentStatement) throw new Exception("Expected assignment statement.");
            string type = walkExpressionIdentifier(root.Params[0], true, false);
            string resultType = walk(root.Params[1]);
            if (resultType != type) throw new Exception($"Can't assign value of type '{resultType}' to variable of type '{type}'");
        }

        string walkExpressionOperator(ASTNode root) {
            if (root.Type != ASTType.ExpressionOperator) throw new Exception("Expected expression operator.");

            // For now, we only have the type "uint8" so all operations will return that type.
            // We just need to check the operands
            ASTNode left = root.Params[0];
            string leftType = "void";
            if (left.Type == ASTType.ExpressionOperator) {
                leftType = walkExpressionOperator(left);
            } else if (left.Type == ASTType.ExpressionNumber) {
                leftType = walkExpressionNumber(left);
            } else if (left.Type == ASTType.ExpressionIdentifier) {
                leftType = walkExpressionIdentifier(left, false, true);
            }
            if (leftType == "void") throw new Exception("Argument can't be void.");

            if (root.Value != "!") {
                ASTNode right = root.Params[1];
                string rightType = "void";
                if (right.Type == ASTType.ExpressionOperator) {
                    rightType = walkExpressionOperator(right);
                } else if (right.Type == ASTType.ExpressionNumber) {
                    rightType = walkExpressionNumber(right);
                } else if (right.Type == ASTType.ExpressionIdentifier) {
                    rightType = walkExpressionIdentifier(right, false, true);
                }
                if (rightType == "void") throw new Exception("Argument can't be void.");
            }

            return "uint8";
        }

        string walkExpressionNumber(ASTNode root) {
            if (root.Type != ASTType.ExpressionNumber) throw new Exception("Expected number literal.");
            return "uint8";
        }

        string walkExpressionIdentifier(ASTNode root, bool isWriting, bool isReading) {
            if (root.Type != ASTType.ExpressionIdentifier) throw new Exception("Expected identifier");
            string name = root.Value;
            if (!knownVariables.ContainsKey(name)) throw new Exception($"Unknown variable name '{name}'");
            VariableOptions variable = knownVariables[name];
            if (isWriting && !variable.IsWritable) throw new Exception($"Variable '{name}' is not writable.");
            if (isReading && !variable.IsReadable) throw new Exception($"Variable '{name}' is not readable.");
            return variable.TypeName;
        }

        void walkExpressionIdexer(ASTNode root) {

        }

    }
}
