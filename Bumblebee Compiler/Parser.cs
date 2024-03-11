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
     *  Params = [ExpressionIdentifier: name, (optional)ExpressionAssignment: value]
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
     *  
     *  ExpressionStatement:
     *      3;
     *      2 * (3 + 5);
     *      func(params);
     *  value = null
     *  params[0] = Expression
     *  
     *  Expression:
     *      3;
     *      2 * (3 + 5);
     *      func(params);
     *  value = null
     *  params[0] = ExpressionOperator or ExpressionNumber or ExpressionIdentifier or ExpressionIndexer or Expression: value
     *      
     *      
     *  ExpressionOperator
     *  Value = "+" or "-" or "*" or "/" or "and" or "or" or "xor" or "!" or "%" or ">>" or "<<"
     *  params[0] = ExpressionOperator or ExpressionNumber or ExpressionIdentifier or ExpressionIndexer: argument 1
     *  params[1] (Optional based on operator) = ExpressionOperator or ExpressionNumber or ExpressionIdentifier or ExpressionIndexer: argument 2
     *  
     *  
     *  ExpressionNumber
     *  Value = number literal
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
     */
    internal class ASTNode {
        internal ASTType Type;
        internal List<ASTNode> Params;
        internal string Value;

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

    internal enum ASTType {
        StatementBlock,
        DeclarationStatement,
        ExpressionAssignmentStatement,
        //ExpressionStatement,

        ExpressionOperator,
        ExpressionNumber,
        ExpressionIdentifier,
        ExpressionIndexer,
        Comment,
/*
        SelectionStatement,
        IterationStatement,
        JumpStatement*/
    }

    internal class Parser {

        internal ASTNode AST; // Abstract Syntax Tree
        private int current = 0;

        internal Parser() {
            AST = new ASTNode(ASTType.StatementBlock, "Program");
        }

        internal void SyntaxAnalyzer(List<Token> tokens) {
            current = 0;
            AST = new ASTNode(ASTType.StatementBlock, "Program");
            while (current < tokens.Count) {
                AST.Params.Add(walkStatement(tokens));
            }
        }

        private ASTNode walkStatement(List<Token> tokens) {
            Token token = tokens[current];
            if (token.Type == TokenType.Identifier && token.Value == "uint8") {
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

        private ASTNode walkStatementBlock(List<Token> tokens) {
            Token token = tokens[current];
            if (token.Type != TokenType.Paren || token.Value != "{") throw new Exception("Invalid statement block. {");
            token = tokens[++current];
            ASTNode statementBlock = new ASTNode(ASTType.StatementBlock);
            while (current < tokens.Count) {
                if (token.Type == TokenType.Paren && token.Value == "}") {
                    break;
                }
                AST.Params.Add(walkStatement(tokens));
                token = tokens[++current];
            }

            if (token.Type != TokenType.Paren || token.Value != "}") throw new Exception("Expected statement block closing bracket. }");
            current++;
            return statementBlock;
        }

        private ASTNode walkDeclarationStatement(List<Token> tokens) {
            Token token = tokens[current];
            if (token.Type != TokenType.Identifier || token.Value != "uint8") throw new Exception("Invalid declaration statement.");
            // Look ahead to see if there's an optional initialization
            if (tokens[current + 2].Type == TokenType.LineDelimiter) {
                // Simple declaration
                ASTNode statement = new ASTNode(ASTType.DeclarationStatement, "uint8");
                token = tokens[++current];
                statement.Params.Add(walkExpressionIdentifier(tokens));
                token = tokens[current];
                // Double check there is a line delimiter
                if (token.Type != TokenType.LineDelimiter) throw new Exception("Line end ';' expected.");
                current++;
                return statement;
            } else {
                // Must be an expression initialization.
                ASTNode statement = new ASTNode(ASTType.DeclarationStatement, "uint8");
                token = tokens[++current];
                int oldIndex = current; // I will explain this in a bit
                statement.Params.Add(walkExpressionIdentifier(tokens));
                current = oldIndex; // Now that we read the name for the declaration, we need to re-parse the name in the ExpressionAssignmentStatement
                statement.Params.Add(walkExpressionAssignmentStatement(tokens));
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
            statement.Params.Add(walkExpressionIdentifier(tokens));
            Token token = tokens[current];
            if (token.Type != TokenType.Operator || token.Value != "=") throw new Exception("Invalid ExpressionAssignmentStatement.");
            current++;
            ASTNode expression = walkExpressionStatement(tokens);
            /*if (expression.Type != ASTType.ExpressionOperator && expression.Type != ASTType.ExpressionNumber && expression.Type != ASTType.ExpressionIdentifier && expression.Type != ASTType.ExpressionIndexer) {
                throw new Exception("Invalid assignment expression type.");
            }*/
            // NOTE: ExpressionStatement will consule the line delimiter for us
            statement.Params.Add(expression);
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

        private ASTNode walkExpression(List<Token> tokens) {
            Token token = tokens[current];

            // TODO check for function call

            ASTNode left;
            // Get first argument
            if (token.Type == TokenType.Paren && token.Value == "(") {
                current++;
                left = walkExpressionStatement(tokens);
                token = tokens[current];
                if (token.Type != TokenType.Paren || token.Value != ")") throw new Exception("Expected closing parenth. )");
                token = tokens[++current];
            } else if (token.Type == TokenType.Number) {
                left = new ASTNode(ASTType.ExpressionNumber, token.Value);
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
            if (token.Type == TokenType.LineDelimiter || (token.Type == TokenType.Paren && token.Value == ")")) {
                return left;
            }

            // Must be an operator
            return walkOperator(tokens, left);
        }

        private ASTNode walkOperator(List<Token> tokens, ASTNode left) {
            Token token = tokens[current]; 

            if (token.Type != TokenType.Operator) throw new Exception("Operator expected, invalid expression.");
            ASTNode op = new ASTNode(ASTType.ExpressionOperator, token.Value);
            op.Params.Add(left);
            current++;
            op.Params.Add(walkExpression(tokens));
            // TODO order of operations? Just check if left or right is also an operator, and restructure tree as needed?
            return op;
        }

    }
}
