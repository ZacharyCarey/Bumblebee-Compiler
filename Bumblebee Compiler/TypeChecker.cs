//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Bumblebee_Compiler {
//    internal class TypeChecker {

//        private Stack<Dictionary<string, VariableOptions>> knownVariables = new();
//        private Dictionary<string, FunctionOptions> knownFunctions = new();

//        internal void CheckTypes(Parser parser, ICompiler compiler) {
//            this.knownVariables.Push(compiler.KnownVariables);
//            this.knownFunctions = compiler.KnownFunctions;
//            walkFunctionDefinitions(parser.Functions);

//            if (parser.AST.Type != ASTType.StatementBlock) throw new Exception("Expected statement block.");
//            foreach (var node in parser.AST.Params) {
//                walk(node);
//            }

//            walkFunctionImplementations(parser.Functions);
//        }

//        private VariableOptions? FindVariable(string name) {
//            foreach(var context in knownVariables) {
//                foreach(var variable in context) {
//                    if (name == variable.Key) {
//                        return variable.Value;
//                    }
//                }
//            }
//            return null;
//        }

//        void walkFunctionDefinitions(List<ASTNode> functions) {
//            foreach (var function in functions) {
//                if (function.Type != ASTType.FunctionDefinition) throw new Exception("Expected function");
//                if (function.Params[0].Type != ASTType.ExpressionIdentifier) throw new Exception("Expected function name");
//                string funcName = function.Params[0].Value;
//                FunctionOptions opts = new();
//                opts.ReturnType = new TypeName(function.Value);

//                foreach (var arg in function.Params.Skip(2)) {
//                    if (arg.Type != ASTType.FunctionArgument) throw new Exception("Expected argument");
//                    if (arg.Params[0].Type != ASTType.ExpressionIdentifier) throw new Exception("Expected argument name");
//                    opts.ArgumentTypes.Add(new TypeName(arg.Value));
//                }

//                knownFunctions[funcName] = opts;
//            }
//        }

//        void walkFunctionImplementations(List<ASTNode> functions) {
//            foreach (var function in functions) {
//                knownVariables.Push(new());
//                if (function.Type != ASTType.FunctionDefinition) throw new Exception("Expected function");
//                if (function.Params[0].Type != ASTType.ExpressionIdentifier) throw new Exception("Expected function name");
//                string funcName = function.Params[0].Value;

//                foreach (var arg in function.Params.Skip(2)) {
//                    if (arg.Type != ASTType.FunctionArgument) throw new Exception("Expected argument");
//                    if (arg.Params[0].Type != ASTType.ExpressionIdentifier) throw new Exception("Expected argument name");
//                    VariableOptions variable = new();
//                    variable.IsReadable = true;
//                    variable.IsWritable = true;
//                    variable.TypeName = new TypeName(arg.Value);
//                    knownVariables.Peek().Add(arg.Params[0].Value, variable);
//                }

//                // Walk statement block without pushing our variables
//                if (function.Params[1].Type != ASTType.StatementBlock) throw new Exception("Expected statement block.");
//                foreach (var node in function.Params[1].Params) {
//                    walk(node);
//                }

//                knownVariables.Pop();
//            }
//        }

//        TypeName walk(ASTNode root) {
//            switch(root.Type) {
//                case ASTType.FunctionCall:
//                    return walkFunctionCall(root);
//                case ASTType.StatementBlock:
//                    walkStatementBlock(root);
//                    return new TypeName("void");
//                case ASTType.DeclarationStatement:
//                    walkDeclarationStatement(root);
//                    return new TypeName("void");
//                case ASTType.ExpressionAssignmentStatement:
//                    walkExpressionAssignmentStatement(root);
//                    return new TypeName("void");
//                case ASTType.ExpressionOperator:
//                    return walkExpressionOperator(root);
//                case ASTType.NumberLiteral:
//                    return walkNumberLiteral(root);
//                case ASTType.ExpressionIdentifier:
//                    return walkExpressionIdentifier(root, false, true);
//                case ASTType.Comment:
//                    return new TypeName("void");
//                case ASTType.BoolLiteral:
//                    return walkBoolLiteral(root);
//                case ASTType.IterationStatement:
//                    walkIterationStatement(root);
//                    return new TypeName("void");
//                case ASTType.SelectionStatement:
//                    walkSelectionStatement(root);
//                    return new TypeName("void");
//                case ASTType.ExpressionIndexer:
//                    return walkExpressionIndexer(root);
//                case ASTType.FunctionReturn:
//                    return new TypeName("void");
//                default:
//                    throw new Exception("Unknown statement.");
//            }
//        }

//        TypeName walkFunctionCall(ASTNode node) {
//            if (node.Type != ASTType.FunctionCall) throw new Exception("Expected function call.");
//            FunctionOptions func = knownFunctions[node.Value];
//            foreach(var arg in func.ArgumentTypes.Zip(node.Params)) {
//                TypeName argType = walk(arg.Second);
//                if (arg.First != argType) {
//                    throw new Exception("Function call does not match function definition.");
//                }
//            }
//            return func.ReturnType;
//        }

//        void walkStatementBlock(ASTNode root, bool stack = true) {
//            if (root.Type != ASTType.StatementBlock) throw new Exception("Expected statement block.");
//            if (stack) {
//                knownVariables.Push(new());
//            }
//            foreach (var node in root.Params) {
//                walk(node);
//            }
//            if (stack) {
//                knownVariables.Pop();
//            }
//        }

//        void walkDeclarationStatement(ASTNode root) {
//            if (root.Type != ASTType.DeclarationStatement) throw new Exception("Expected declaration statement.");
//            TypeName type = root.ValueType;

//            ASTNode identifier = root.Params[0];
//            if (identifier.Type != ASTType.ExpressionIdentifier) throw new Exception("Expected identifier.");
//            string name = identifier.Value;

//            VariableOptions? variable = FindVariable(name);
//            if (variable != null || knownFunctions.ContainsKey(name)) {
//                throw new Exception($"The identifier '{name}' has been previously declared.");
//            }

//            int index = 1;
//            bool isConst = false;
//            while (index < root.Params.Count && root.Params[index].Type == ASTType.VariableModifier) {
//                ref bool modifier = ref isConst;
//                switch(root.Params[index].Value) {
//                    case "const": modifier = ref isConst; break;
//                    default:
//                        throw new Exception("Unknown modifier");
//                }
//                if (modifier) throw new Exception("Modifier was listed twice");
//                modifier = true;
//                index++;
//            }

//            VariableOptions options = new();
//            options.IsReadable = true;
//            options.IsWritable = !isConst;
//            options.TypeName = type;
//            knownVariables.Peek().Add(name, options);

//            if (index < root.Params.Count) {
//                if (isConst) {
//                    ASTNode assignment = root.Params[index];
//                    if (assignment.Type != ASTType.ExpressionAssignmentStatement) throw new Exception("Expected assignment.");
//                    ASTNode initialize = assignment.Params[1];
//                    if (initialize.Type == ASTType.BoolLiteral) {
//                        if (type.Name != "bool") throw new Exception("initializer does not match type.");
//                    } else if (initialize.Type == ASTType.NumberLiteral) {
//                        if (type.Name != "uint8") throw new Exception("initializer does not match type");
//                    } else {
//                        throw new Exception("Must be a static expression for const value.");
//                    }
//                } else {
//                    if (root.Params[index].Type != ASTType.ExpressionAssignmentStatement) throw new Exception("Expected assignment statement");
//                    walkExpressionAssignmentStatement(root.Params[1]);
//                }

//                index++;
//                if (index < root.Params.Count) {
//                    throw new Exception("Did not expect more arguments.");
//                }
//            } else {
//                if (isConst) throw new Exception("Constant values must be initialized");
//            }
//        }

//        void walkExpressionAssignmentStatement(ASTNode root) {
//            if (root.Type != ASTType.ExpressionAssignmentStatement) throw new Exception("Expected assignment statement.");
//            TypeName type = new();
//            if (root.Params[0].Type == ASTType.ExpressionIdentifier) {
//                type = walkExpressionIdentifier(root.Params[0], true, false);
//            } else if (root.Params[0].Type == ASTType.ExpressionIndexer) {
//                type = walkExpressionIndexer(root.Params[0]);
//            } else {
//                throw new Exception("Unknown type");
//            }
//            TypeName resultType = walk(root.Params[1]);
//            if (resultType != type) throw new Exception($"Can't assign value of type '{resultType}' to variable of type '{type}'");
//        }

//        TypeName walkAccessor(ASTNode root, bool writing, bool reading) {
//            if (root.Type != ASTType.Accessor) throw new Exception("Expected accessor");
//            ASTNode variable = root.Params[0];
//            if (variable.Type != ASTType.ExpressionIdentifier) throw new Exception("Expected identifier");
//            VariableOptions? possibleType = FindVariable(variable.Value);
//            if (possibleType == null) throw new Exception("Unknown variable.");
//            VariableOptions type = (VariableOptions)possibleType;
//            if (!type.TypeName.IsArray) throw new Exception("Expected array variable");
//            //if (reading && !type.IsReadable) throw new Exception("Variable is not readable");
//            //if (writing && !type.IsWritable) throw new Exception("Variable is not writable");

//            if (root.Value == "Length") {
//                int len = type.TypeName.ArrayLength;
//                //TODO if (len < 0) throw new Exception("Invalid length!");
//                return new TypeName("uint8");
//            }

//            return type.TypeName;
//        }

//        TypeName walkExpressionOperator(ASTNode root) {
//            if (root.Type != ASTType.ExpressionOperator) throw new Exception("Expected expression operator.");

//            // For now, we only have the type "uint8" so all operations will return that type.
//            // We just need to check the operands
//            ASTNode left = root.Params[0];
//            TypeName leftType = new("void");
//            if (left.Type == ASTType.ExpressionOperator) {
//                leftType = walkExpressionOperator(left);
//            } else if (left.Type == ASTType.NumberLiteral) {
//                leftType = walkNumberLiteral(left);
//            } else if (left.Type == ASTType.BoolLiteral) {
//                leftType = walkBoolLiteral(left);
//            } else if (left.Type == ASTType.ExpressionIdentifier) {
//                leftType = walkExpressionIdentifier(left, false, true);
//            } else if (left.Type == ASTType.Accessor) {
//                leftType = walkAccessor(left, false, true);
//            } else if (left.Type == ASTType.ExpressionIndexer) {
//                leftType = walkExpressionIndexer(left);
//            } else {
//                throw new Exception("Unknown type");
//            }
//            if (leftType == new TypeName("void")) throw new Exception("Argument can't be void.");

//            TypeName rightType = new("void");
//            if (root.Value != "~" && root.Value != "not") {
//                ASTNode right = root.Params[1];
//                if (right.Type == ASTType.ExpressionOperator) {
//                    rightType = walkExpressionOperator(right);
//                } else if (right.Type == ASTType.NumberLiteral) {
//                    rightType = walkNumberLiteral(right);
//                } else if (right.Type == ASTType.BoolLiteral) {
//                    rightType = walkBoolLiteral(right);
//                } else if (right.Type == ASTType.ExpressionIdentifier) {
//                    rightType = walkExpressionIdentifier(right, false, true);
//                } else if (right.Type == ASTType.Accessor) {
//                    rightType = walkAccessor(right, false, true);
//                } else if (right.Type == ASTType.ExpressionIndexer) {
//                    rightType = walkExpressionIndexer(right);
//                } else {
//                    throw new Exception("Unknown type");
//                }
//                if (rightType == new TypeName("void")) throw new Exception("Argument can't be void.");
//            }

//            switch(root.Value) {
//                case "+":
//                case "-":
//                case "*":
//                case "/":
//                case "%":
//                case "&":
//                case "|":
//                case "^":
//                case ">>":
//                case "<<":
//                    if (leftType != new TypeName("uint8")) throw new Exception($"Operator {root.Value} requires int type operand.");
//                    if (rightType != new TypeName("uint8")) throw new Exception($"Operator {root.Value} requires int type operand.");
//                    return new TypeName("uint8");
//                case "~":
//                    if (leftType != new TypeName("uint8")) throw new Exception($"Operator {root.Value} requires int type operand.");
//                    return new TypeName("uint8");
//                case "and":
//                case "or":
//                case "xor":
//                    if (leftType != new TypeName("bool")) throw new Exception($"Operator {root.Value} requires bool type operand.");
//                    if (rightType != new TypeName("bool")) throw new Exception($"Operator {root.Value} requires bool type operand.");
//                    return new TypeName("bool");
//                case "not":
//                    if (leftType != new TypeName("bool")) throw new Exception($"Operator {root.Value} requires bool type operand.");
//                    return new TypeName("bool");
//                case ">":
//                case "<":
//                case "<=":
//                case ">=":
//                case "==":
//                case "!=":
//                    if (leftType != rightType) throw new Exception($"Operator {root.Value} must be the same type.");
//                    return new TypeName("bool");
//                default:
//                    throw new Exception("Unknown operator");
//            }
//        }

//        TypeName walkNumberLiteral(ASTNode root) {
//            if (root.Type != ASTType.NumberLiteral) throw new Exception("Expected number literal.");
//            return new TypeName("uint8");
//        }

//        TypeName walkBoolLiteral(ASTNode root) {
//            if (root.Type != ASTType.BoolLiteral) throw new Exception("Expected bool literal.");
//            return new TypeName("bool");
//        }

//        TypeName walkExpressionIdentifier(ASTNode root, bool isWriting, bool isReading) {
//            if (root.Type == ASTType.ExpressionIdentifier) {
//                string name = root.Value;

//                VariableOptions? knownVariable = FindVariable(name);
//                if (knownVariable == null) throw new Exception($"Unknown variable name '{name}'");
//                VariableOptions variable = (VariableOptions)knownVariable;

//                if (isWriting && !variable.IsWritable) throw new Exception($"Variable '{name}' is not writable.");
//                if (isReading && !variable.IsReadable) throw new Exception($"Variable '{name}' is not readable.");
//                return variable.TypeName;
//            } else {
//                throw new Exception("Invalid expression identifier");
//            }
//        }

//        private TypeName getConditionType(ASTNode param) {
//            switch (param.Type) {
//                case ASTType.BoolLiteral: return walkBoolLiteral(param);
//                case ASTType.ExpressionOperator: return walkExpressionOperator(param);
//                case ASTType.ExpressionIdentifier: return walkExpressionIdentifier(param, false, true);
//                case ASTType.FunctionCall:
//                    return knownFunctions[param.Value].ReturnType;
//                    // TODO expression indexer
//                default:
//                    throw new Exception("Invalid condition.");
//            }
//        }

//        void walkIterationStatement(ASTNode root) {
//            if (root.Type != ASTType.IterationStatement) throw new Exception("Iteration expected");
//            ASTNode statement;
//            if (root.Value == "while") {
//                TypeName conditionType = getConditionType(root.Params[0]);
//                if (conditionType != new TypeName("bool")) throw new Exception("Condition must be a bool type.");
//                statement = root.Params[1];
//                walkStatementBlock(statement);
//            } else if (root.Value == "for") {
//                knownVariables.Push(new());

//                if (root.Params[0] != null) {
//                    walkDeclarationStatement(root.Params[0]);
//                }

//                TypeName conditionType = getConditionType(root.Params[1]);
//                if (conditionType != new TypeName("bool")) throw new Exception("Condition must be a bool type");

//                if (root.Params[2] != null) {
//                    walk(root.Params[2]);
//                }

//                statement = root.Params[3];

//                walkStatementBlock(statement, false);
//                knownVariables.Pop();
//            } else {
//                throw new Exception("Unknown iteration");
//            }
//        }

//        void walkSelectionStatement(ASTNode root) {
//            if (root.Type != ASTType.SelectionStatement) throw new Exception("Selection expected");
//            if (root.Value != "if") throw new Exception("Unknown selection");

//            TypeName conditionType = getConditionType(root.Params[0]);
//            if (conditionType != new TypeName("bool")) throw new Exception("Condition must be a bool type.");

//            walkStatementBlock(root.Params[1]);

//            if (root.Params.Count > 2) {
//                if (root.Params[2].Type == ASTType.SelectionStatement) {
//                    walkSelectionStatement(root.Params[2]);
//                } else {
//                    walkStatementBlock(root.Params[2]);
//                }
//            }
//        }

//        TypeName walkExpressionIndexer(ASTNode root) {
//            if (root.Type != ASTType.ExpressionIndexer) throw new Exception("Expected indexer");
//            TypeName type = walk(root.Params[0]);
//            if (type != new TypeName("uint8")) throw new Exception("Indexer must be an int");
//            return new TypeName("uint8");
//        }

//    }
//}
