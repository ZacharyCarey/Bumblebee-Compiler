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
                case ASTType.NumberLiteral:
                    return walkNumberLiteral(root);
                case ASTType.ExpressionIdentifier:
                    return walkExpressionIdentifier(root, false, true);
                case ASTType.Comment:
                    return "void";
                case ASTType.BoolLiteral:
                    return walkBoolLiteral(root);
                case ASTType.IterationStatement:
                    walkIterationStatement(root);
                    return "void";
                case ASTType.SelectionStatement:
                    walkSelectionStatement(root);
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
            } else if (left.Type == ASTType.NumberLiteral) {
                leftType = walkNumberLiteral(left);
            } else if (left.Type == ASTType.BoolLiteral) {
                leftType = walkBoolLiteral(left);
            } else if (left.Type == ASTType.ExpressionIdentifier) {
                leftType = walkExpressionIdentifier(left, false, true);
            }
            if (leftType == "void") throw new Exception("Argument can't be void.");

            string rightType = "void";
            if (root.Value != "~" && root.Value != "not") {
                ASTNode right = root.Params[1];
                if (right.Type == ASTType.ExpressionOperator) {
                    rightType = walkExpressionOperator(right);
                } else if (right.Type == ASTType.NumberLiteral) {
                    rightType = walkNumberLiteral(right);
                } else if (right.Type == ASTType.BoolLiteral) {
                    rightType = walkBoolLiteral(right);
                } else if (right.Type == ASTType.ExpressionIdentifier) {
                    rightType = walkExpressionIdentifier(right, false, true);
                }
                if (rightType == "void") throw new Exception("Argument can't be void.");
            }

            switch(root.Value) {
                case "+":
                case "-":
                case "*":
                case "/":
                case "%":
                case "&":
                case "|":
                case "^":
                case ">>":
                case "<<":
                    if (leftType != "uint8") throw new Exception($"Operator {root.Value} requires int type operand.");
                    if (rightType != "uint8") throw new Exception($"Operator {root.Value} requires int type operand.");
                    return "uint8";
                case "~":
                    if (leftType != "uint8") throw new Exception($"Operator {root.Value} requires int type operand.");
                    return "uint8";
                case "and":
                case "or":
                case "xor":
                    if (leftType != "bool") throw new Exception($"Operator {root.Value} requires bool type operand.");
                    if (rightType != "bool") throw new Exception($"Operator {root.Value} requires bool type operand.");
                    return "bool";
                case "not":
                    if (leftType != "bool") throw new Exception($"Operator {root.Value} requires bool type operand.");
                    return "bool";
                case ">":
                case "<":
                case "<=":
                case ">=":
                case "==":
                case "!=":
                    if (leftType != rightType) throw new Exception($"Operator {root.Value} must be the same type.");
                    return "bool";
                default:
                    throw new Exception("Unknown operator");
            }
        }

        string walkNumberLiteral(ASTNode root) {
            if (root.Type != ASTType.NumberLiteral) throw new Exception("Expected number literal.");
            return "uint8";
        }

        string walkBoolLiteral(ASTNode root) {
            if (root.Type != ASTType.BoolLiteral) throw new Exception("Expected bool literal.");
            return "bool";
        }

        string walkExpressionIdentifier(ASTNode root, bool isWriting, bool isReading) {
            if (root.Type != ASTType.ExpressionIdentifier) throw new Exception("Expected identifier");
            string name = root.Value;

            VariableOptions variable = new();
 /*           if (name == "false" || name == "true") {
                variable.IsReadable = true;
                variable.IsWritable = false;
                variable.TypeName = "bool";
            } else {*/
                if (!knownVariables.ContainsKey(name)) throw new Exception($"Unknown variable name '{name}'");
                variable = knownVariables[name];
            //}

            if (isWriting && !variable.IsWritable) throw new Exception($"Variable '{name}' is not writable.");
            if (isReading && !variable.IsReadable) throw new Exception($"Variable '{name}' is not readable.");
            return variable.TypeName;
        }

        private string getConditionType(ASTNode param) {
            switch (param.Type) {
                case ASTType.BoolLiteral: return walkBoolLiteral(param);
                case ASTType.ExpressionOperator: return walkExpressionOperator(param);
                case ASTType.ExpressionIdentifier: return walkExpressionIdentifier(param, false, true);
                // TODO expression indexer
                default:
                    throw new Exception("Invalid condition.");
            }
        }

        void walkIterationStatement(ASTNode root) {
            if (root.Type != ASTType.IterationStatement) throw new Exception("Iteration expected");
            if (root.Value != "while") throw new Exception("Unknown iteration");

            string conditionType = getConditionType(root.Params[0]);
            if (conditionType != "bool") throw new Exception("Condition must be a bool type.");

            walkStatementBlock(root.Params[1]);
        }

        void walkSelectionStatement(ASTNode root) {
            if (root.Type != ASTType.SelectionStatement) throw new Exception("Selection expected");
            if (root.Value != "if") throw new Exception("Unknown selection");

            string conditionType = getConditionType(root.Params[0]);
            if (conditionType != "bool") throw new Exception("Condition must be a bool type.");

            walkStatementBlock(root.Params[1]);

            if (root.Params.Count > 2) {
                if (root.Params[2].Type == ASTType.SelectionStatement) {
                    walkSelectionStatement(root.Params[2]);
                } else {
                    walkStatementBlock(root.Params[2]);
                }
            }
        }

        void walkExpressionIndexer(ASTNode root) {

        }

    }
}
