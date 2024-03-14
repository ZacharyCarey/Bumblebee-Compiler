using Bumblebee_Compiler.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler {

    /*
     * Declaration statement: 
     *      uint8 radius;
     *      uint8 radius = 2;
     *      uint8 radius = 2 * (3 + 5);
     *  Value = type(uint8)
     *  Params[0] = ExpressionIdentifier: name, 
     *  Params[n] = VariableModifier: modifiers
     *  Params[n+1] = (optional)ExpressionAssignment: value
     *  i.e. Declares a new variable (with type and name). Uses optional expression for in-line initialization.
     *  This could be a const number literal, or an expression that needs evaluated.
     *  When expression statement is used, it can be though of as a second line of code. This node
     *  is declaring the variable as one statement, then the first child node is an expression statement
     *  which is assigning a new value to the variable.
     *  
     *  ExpressionAssignmentStatement: 
     *      radius = 3;
     *      radius = 2 * (3 + 5);
     *  Value = null
     *  params[0] = ExpressionIdentifier or ExpressionIndexer: target save location
     *  params[1] = ExpressionStatement
     *  
     *  DEFINITION:
     *  Expression:
     *      3;
     *      2 * (3 + 5);
     *      func(params);
     *  Expression: ExpressionOperator or NumberLiteral or BoolLiteral, ExpressionIdentifier or ExpressionIndexer: value
     *  
     *      
     *      
     *  ExpressionOperator
     *  Value = "+", "-", "*", "/", "%", ">>", "<<"
     *          "and", "or", "xor", "not", 
     *          "&", "|", "^", "~" 
     *  params[0] = ExpressionOperator or ExpressionNumber or ExpressionIdentifier or ExpressionIndexer: argument 1
     *  params[1] (Optional based on operator) = ExpressionOperator or ExpressionNumber or ExpressionIdentifier or ExpressionIndexer: argument 2
     *  
     *  
     *  NumberLiteral
     *  Value = number literal
     *  
     *  BoolLiteral
     *  Value = "true" or "false"
     *  
     *  
     *  ExpressionIdentifier
     *  Value = string name
     *  Usually a variable, but can be used to identify functions as well
     *  
     *  
     *  ExpressionIndexer
     *  Value = string name
     *  params[0] = ExpressionOperator or ExpressionNumber or ExpressionIdentifier or ExpressionIndexer: the index to access
     * 
     *  StatementBlock
     *  Value = null
     *  params[n] = DeclarationStatement or ExpressionAssignmentStatement or ExpressionStatement or StatementBlock
     *  
     *  
     *  
     *  IterationStatement
     *  Value = "while"
     *  params[0] = Condition: BoolLiteral, ExpressionOperator, ExpressionIdentifier, ExpressionIndexer
     *  params[1] = Code block: StatementBlock
     *  
     *  
     *  
     *  SelectionStatement
     *  Value = "if" or "else"
     *  params[0] = Condition: BoolLiteral, ExpressionOperator, ExpressionIdentifier, ExpressionIndexer
     *  params[1] = Code block: StatementBlock
     *  Params[2] = (Optional in presence of 'else') Code block: StatementBlock or SelectionStatement
     *  
     *  
     *  FunctionDefinition
     *  Value = "ReturnValue"
     *  params[0] = ExpressionIdentifier functionName
     *  params[1] = StatementBlock
     *  params[n] = FunctionArgument parameters
     *  
     *  
     *  FunctionCall
     *  value = "FuncName"
     *  params[n] = Expressions
     *  
     *  
     *  FunctionReturn
     *  params[0] = (optional)Expression returnValue
     *  
     *  
     *  FunctionArgument
     *  value = "ArgumentType"
     *  params[0] = ExpressionIdentifier
     */
    public class ASTNode {
        public ASTType Type;
        public List<ASTNode> Params;
        public string Value;

        internal ASTNode(ASTType type) {
            this.Type = type;
            Params = new();
            //Value = "";
        }

        internal ASTNode(ASTType type, string value) {
            this.Type = type;
            this.Params = new();
            this.Value = value;
        }
    }

    public enum ASTType {
        StatementBlock,
        DeclarationStatement,
        ExpressionAssignmentStatement,
        IterationStatement,
        SelectionStatement,

        ExpressionOperator,
        NumberLiteral,
        BoolLiteral,
        ExpressionIdentifier,
        ExpressionIndexer,
        Comment,
        FunctionCall,
        FunctionDefinition,
        FunctionArgument,
        FunctionReturn,
        VariableModifier
    }

    public class Parser {

        public ASTNode AST; // Abstract Syntax Tree
        public List<ASTNode> Functions;
        private int current = 0;

        internal Parser() {
            AST = new ASTNode(ASTType.StatementBlock, "Program");
        }

        internal void SyntaxAnalyzer(List<Token> tokens) {
            current = 0;
            Functions = new();
            AST = new ASTNode(ASTType.StatementBlock, "Program");
            while (current < tokens.Count) {
                Token debug = tokens[current];
                ASTNode node = walkStatement(tokens);
                //if (node == null) throw new Exception("Cant add null node");
                if (node != null) {
                    AST.Params.Add(node);
                }
            }
        }

        private bool IsValidType(string name) {
            // TODO instead of looking for type for declaration,
            // should look for two identifiers followed by an "=" then assume first is type and 2nd is name.
            switch(name) {
                case "uint8":
                case "bool":
                    return true;
                default:
                    return false;
            }
        }

        private bool isFunctionDefinition(List<Token> tokens) {
            if (tokens[current].Type != TokenType.Identifier) return false;
            if (current + 1 >= tokens.Count || tokens[current + 1].Type != TokenType.Identifier) return false;
            if (current + 2 >= tokens.Count || tokens[current + 2].Type != TokenType.Paren) return false;
            if (tokens[current + 2].Value != "(") return false;
            return true;
        }

        private bool isFunctionCall(List<Token> tokens) {
            if (tokens[current].Type != TokenType.Identifier) return false;
            if (current + 1 >= tokens.Count || tokens[current + 1].Type != TokenType.Paren) return false;
            if (tokens[current + 1].Value != "(") return false;
            return true;
        }

        private ASTNode walkStatement(List<Token> tokens) {
            Token token = tokens[current];
            if (isFunctionDefinition(tokens)) {
                walkFunctionDefinition(tokens);
                return null;
            }
            if (isFunctionCall(tokens)) {
                ASTNode node = walkFunctionCall(tokens);
                if (tokens[current].Type != TokenType.LineDelimiter) throw new Exception("Expected line delimiter");
                current++;
                return node;
            }
            if (token.Type == TokenType.Identifier && token.Value == "ret") {
                return walkFunctionReturn(tokens);
            }
            if (token.Type == TokenType.VariableModifier || (token.Type == TokenType.Identifier && IsValidType(token.Value))) {
                return walkDeclarationStatement(tokens);
            }
            if (token.Type == TokenType.Paren && token.Value == "{") {
                return walkStatementBlock(tokens);
            }
            if (token.Type == TokenType.Comment) {
                ASTNode comment = new ASTNode(ASTType.Comment, token.Value);
                current++;
                return comment;
            }
            if (token.Type == TokenType.Iteration) {
                return walkIteration(tokens);
            }
            if (token.Type == TokenType.Selection) {
                return walkSelection(tokens);
            }

            // Attempt to search for expression, or expression statement
            for(int i = current; i < tokens.Count; i++) {
                if (tokens[i].Type == TokenType.Operator && tokens[i].Value == "=") {
                    return walkExpressionAssignmentStatement(tokens);
                }
                if (tokens[i].Type == TokenType.LineDelimiter) {
                    return walkExpressionStatement(tokens);
                }
            }

            throw new Exception("Invalid statement.");
        }

        private ASTNode walkIteration(List<Token> tokens) {
            Token token = tokens[current];
            if (token.Type != TokenType.Iteration) throw new Exception("Expected iteration.");

            ASTNode node = new ASTNode(ASTType.IterationStatement, token.Value);
            if (token.Value == "while") {
                token = tokens[++current];
                if (token.Type != TokenType.Paren || token.Value != "(") throw new Exception("Expected loop condition");
                ASTNode exp1 = walkExpression(tokens, true);
                if (exp1 == null) throw new Exception("Cant add null node");
                node.Params.Add(exp1);
                token = tokens[current];
                ASTNode exp2 = walkStatementBlock(tokens);
                if (exp2 == null) throw new Exception("Cant add null node");
                node.Params.Add(exp2);
                return node;
            }

            throw new Exception("Invalid iteration.");
        }

        private ASTNode walkSelection(List<Token> tokens) {
            Token token = tokens[current];
            if (token.Type != TokenType.Selection) throw new Exception("Expected selection.");
            if (token.Value != "if") throw new Exception("Invalid selection statement.");

            ASTNode node = new ASTNode(ASTType.SelectionStatement, token.Value);
            token = tokens[++current];
            if (token.Type != TokenType.Paren || token.Value != "(") throw new Exception("Expected loop condition");
            ASTNode node1 = walkExpression(tokens, true);
            ASTNode node2 = walkStatementBlock(tokens);
            if (node1 == null || node2 == null) throw new Exception("Cant add null node");
            node.Params.Add(node1);
            node.Params.Add(node2);
            token = tokens[current];
            if (token.Type == TokenType.Selection && token.Value == "else") {
                token = tokens[++current];
                if (token.Type == TokenType.Selection && token.Value == "if") {
                    ASTNode node3 = walkSelection(tokens);
                    if (node3 == null) throw new Exception("Cant add null node");
                    node.Params.Add(node3);
                } else {
                    ASTNode node3 = walkStatementBlock(tokens);
                    if (node3 == null) throw new Exception("Cant add null node");
                    node.Params.Add(node3);
                }
            }
            return node;
        }

        private ASTNode walkStatementBlock(List<Token> tokens) {
            Token token = tokens[current];
            if (token.Type != TokenType.Paren || token.Value != "{") throw new Exception("Invalid statement block. {");
            token = tokens[++current];
            ASTNode statementBlock = new ASTNode(ASTType.StatementBlock);
            while (current < tokens.Count) {
                if (token.Type == TokenType.Paren && token.Value == "}") {
                    break;
                }
                ASTNode node1 = walkStatement(tokens);
                if (node1 == null) throw new Exception("Cant add null node");
                statementBlock.Params.Add(node1);
                token = tokens[current];
            }

            if (token.Type != TokenType.Paren || token.Value != "}") throw new Exception("Expected statement block closing bracket. }");
            current++;
            return statementBlock;
        }

        private ASTNode walkDeclarationStatement(List<Token> tokens) {
            Token token = tokens[current];

            // Walk any number of modifiers
            List<ASTNode> modifiers = new();
            while (token.Type == TokenType.VariableModifier) {
                modifiers.Add(new ASTNode(ASTType.VariableModifier, token.Value));
                token = tokens[++current];
            }

            if (token.Type != TokenType.Identifier || !IsValidType(token.Value)) throw new Exception("Invalid declaration statement.");
            
            string type = token.Value;
            // Look ahead to see if there's an optional initialization
            if (tokens[current + 2].Type == TokenType.LineDelimiter) {
                // Simple declaration
                ASTNode statement = new ASTNode(ASTType.DeclarationStatement, type);
                token = tokens[++current];
                ASTNode node1 = walkExpressionIdentifier(tokens);
                if (node1 == null) throw new Exception("Cant add null node");
                statement.Params.Add(node1);
                token = tokens[current];
                statement.Params.AddRange(modifiers);
                // Double check there is a line delimiter
                if (token.Type != TokenType.LineDelimiter) throw new Exception("Line end ';' expected.");
                current++;
                return statement;
            } else {
                // Must be an expression initialization.
                ASTNode statement = new ASTNode(ASTType.DeclarationStatement, type);
                token = tokens[++current];
                int oldIndex = current; // I will explain this in a bit
                ASTNode node1 = walkExpressionIdentifier(tokens);
                statement.Params.Add(node1);
                current = oldIndex; // Now that we read the name for the declaration, we need to re-parse the name in the ExpressionAssignmentStatement
                statement.Params.AddRange(modifiers);
                ASTNode node2 = walkExpressionAssignmentStatement(tokens);
                statement.Params.Add(node2);
                if (node1 == null || node2 == null) throw new Exception("Cant add null node");
                // Note: ExpressionAssignmentStatement will have checked for the line delimitor already
                return statement;
            }
        }

        private ASTNode walkExpressionIdentifier(List<Token> tokens) {
            Token token = tokens[current];
            if (token.Type != TokenType.Identifier) throw new Exception("Invalid identifier.");
            current++;
            return new ASTNode(ASTType.ExpressionIdentifier, token.Value);
        }

        private ASTNode walkExpressionAssignmentStatement(List<Token> tokens) {
            ASTNode statement = new ASTNode(ASTType.ExpressionAssignmentStatement);
            ASTNode node1 = walkExpressionIdentifier(tokens);
            statement.Params.Add(node1);
            Token token = tokens[current];
            if (token.Type != TokenType.Operator || token.Value != "=") throw new Exception("Invalid ExpressionAssignmentStatement.");
            current++;
            ASTNode expression = walkExpressionStatement(tokens);
            /*if (expression.Type != ASTType.ExpressionOperator && expression.Type != ASTType.ExpressionNumber && expression.Type != ASTType.ExpressionIdentifier && expression.Type != ASTType.ExpressionIndexer) {
                throw new Exception("Invalid assignment expression type.");
            }*/
            // NOTE: ExpressionStatement will consule the line delimiter for us
            statement.Params.Add(expression);
            if (node1 == null || expression == null) throw new Exception("Cant add null node");
            return statement;
        }

        private ASTNode walkExpressionStatement(List<Token> tokens) {
            ASTNode statement = walkExpression(tokens); //new ASTNode(ASTType.ExpressionStatement);
            //statement.Params.Add(walkExpression(tokens));
            Token token = tokens[current];
            if (token.Type != TokenType.LineDelimiter) throw new Exception("End of statement expected.");
            current++;
            return statement;
        }

        private ASTNode walkExpression(List<Token> tokens, bool single = false) {
            Token token = tokens[current];

            ASTNode left;
            // Get first argument
            if (isFunctionCall(tokens)) {
                left = walkFunctionCall(tokens);
                token = tokens[current];
            } else if (token.Type == TokenType.Operator && (token.Value == "not" || token.Value == "~")) {
                left = walkOperator(tokens, null);
                token = tokens[current];
            } else if (token.Type == TokenType.Paren && token.Value == "(") {
                current++;
                left = walkExpression(tokens);
                token = tokens[current];
                if (token.Type != TokenType.Paren || token.Value != ")") throw new Exception("Expected closing parenth. )");
                token = tokens[++current];
            } else if (token.Type == TokenType.NumberLiteral) {
                left = new ASTNode(ASTType.NumberLiteral, token.Value);
                token = tokens[++current];
            } else if (token.Type == TokenType.BoolLiteral) {
                left = new ASTNode(ASTType.BoolLiteral, token.Value);
                token = tokens[++current];
            } else if (token.Type == TokenType.CharLiteral) {
                left = new ASTNode(ASTType.NumberLiteral, ((int)token.Value[0]).ToString());
                token = tokens[++current];
            } else if (token.Type == TokenType.Identifier) {
                // TODO check for function
                left = walkExpressionIdentifier(tokens);
                token = tokens[current];
            } // TODO indexer
            else {
                throw new Exception("Invalid expression.");
            }

            // Check for either operator or delimiter
            if (single || token.Type == TokenType.LineDelimiter || (token.Type == TokenType.Paren && token.Value == ")") || token.Type == TokenType.ArgumentSeparator) {
                return left;
            }

            // Must be an operator
            return walkOperator(tokens, left);
        }

        private ASTNode walkOperator(List<Token> tokens, ASTNode left) {
            Token token = tokens[current]; 

            if (token.Type != TokenType.Operator) throw new Exception("Operator expected, invalid expression.");
            ASTNode op = new ASTNode(ASTType.ExpressionOperator, token.Value);
            if (token.Value == "not" || token.Value == "~") {
                // Left is null, we need to get the arg
                ASTNode node1 = walkExpression(tokens, true);
                op.Params.Add(node1);
                if (node1 == null) throw new Exception("Cant add null node");
            } else {
                op.Params.Add(left);
                current++;
                ASTNode node1 = walkExpression(tokens);
                op.Params.Add(node1);
                if (node1 == null || left == null) throw new Exception("Cant add null node");
            }
            // TODO order of operations? Just check if left or right is also an operator, and restructure tree as needed?
            return op;
        }

        private void walkFunctionDefinition(List<Token> tokens) {
            Token token = tokens[current];
            if (token.Type != TokenType.Identifier) throw new Exception("Expected function type");
            ASTNode node = new(ASTType.FunctionDefinition, token.Value);

            token = tokens[++current];
            if (token.Type != TokenType.Identifier) throw new Exception("Expected function name");
            ASTNode node1 = walkExpressionIdentifier(tokens);
            node.Params.Add(node1);
            if (node1 == null) throw new Exception("Cant add null node");

            token = tokens[current];
            if (token.Type != TokenType.Paren || token.Value != "(") throw new Exception("Expected function definition.");
            token = tokens[++current];

            // Parse arguments
            List<ASTNode> args = new();
            if (token.Type != TokenType.Paren) {
                while (true) {
                    token = tokens[current];
                    if (token.Type != TokenType.Identifier) throw new Exception("Expected argument type");
                    string type = token.Value;

                    token = tokens[++current];
                    if (token.Type != TokenType.Identifier) throw new Exception("Expected argument name");

                    ASTNode arg = new(ASTType.FunctionArgument, type);
                    ASTNode node2 = walkExpressionIdentifier(tokens);
                    arg.Params.Add(node2);
                    args.Add(arg);
                    if (node2 == null || arg == null) throw new Exception("Cant add null node");

                    token = tokens[current];
                    if (token.Type == TokenType.Paren) break;
                    if (token.Type != TokenType.ArgumentSeparator) throw new Exception("Expected comma argument separater");
                    current++;
                }
            }

            if (current >= tokens.Count) throw new Exception("Expected function definition");
            token = tokens[current];
            if (token.Type != TokenType.Paren || token.Value != ")") throw new Exception("Expected closing parentheses");
            token = tokens[++current];

            // TODO verify value is returned
            if (token.Type != TokenType.Paren || token.Value != "{") throw new Exception("Expected function definition");
            ASTNode node3 = walkStatementBlock(tokens);
            node.Params.Add(node3);
            node.Params.AddRange(args);

            Functions.Add(node);
            if (node3 == null || node == null) throw new Exception("Cant add null node");
        }

        private ASTNode walkFunctionCall(List<Token> tokens) {
            Token token = tokens[current];
            if (token.Type != TokenType.Identifier) throw new Exception("Expected function name");
            ASTNode node = new ASTNode(ASTType.FunctionCall, token.Value);

            token = tokens[++current];
            if (token.Type != TokenType.Paren || token.Value != "(") throw new Exception("Expected function definition.");
            token = tokens[++current];

            // Parse arguments
            List<ASTNode> args = new();
            if (token.Type != TokenType.Paren) {
                while (true) {
                    ASTNode node1 = walkExpression(tokens);
                    node.Params.Add(node1);
                    if (node1 == null) throw new Exception("Cant add null node");
                    token = tokens[current];
                    if (token.Type == TokenType.Paren) break;
                    if (token.Type != TokenType.ArgumentSeparator) throw new Exception("Expected comma argument separater");
                    current++;
                }
            }

            if (current >= tokens.Count) throw new Exception("Expected function call");
            token = tokens[current];
            if (token.Type != TokenType.Paren || token.Value != ")") throw new Exception("Expected closing parentheses");
            token = tokens[++current];

            return node;
        }

        private ASTNode walkFunctionReturn(List<Token> tokens) {
            Token token = tokens[current];
            if (token.Type != TokenType.Identifier || token.Value != "ret") throw new Exception("Expected function return");

            ASTNode node = new ASTNode(ASTType.FunctionReturn);
            token = tokens[++current];

            if (token.Type != TokenType.LineDelimiter) throw new Exception("Function returns are not yet supported. Expected line delimiter");
            return node;
        }
    }
}
