using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bumblebee_Compiler {
    internal class Transformer {

        internal HashSet<string> Includes = new() {
            "windows",
            "user32",
            "kernel32",
            "gdi32",
        };

        internal HashSet<string> Libs = new() {
            "kernel32",
            "user32",
            "gdi32"
        };

        internal HashSet<string> Prototypes = new() {
            "exit proto c, :DWORD"
        };

        internal HashSet<string> StringLiterals = new();

        internal List<ASTNode> Program = new();

        internal void Transform(ASTNode program) {
            walkNode(program, null, this);
        }

        private static void walkNode(ASTNode node, ASTNode parent, Transformer program) {
            Action<ASTNode, ASTNode, Transformer> method;
            if (transformMethods.TryGetValue(node.Type, out method)) {
                method(node, parent, program);
            }
            if (node.Type == "Program") {
                walkNodes(node.Body, node, program);
            } 
        }

        private static void walkNodes(List<ASTNode> nodes, ASTNode parent, Transformer program) {
            foreach (ASTNode node in nodes) {
                walkNode(node, parent, program);
            }
        }

        private static Dictionary<string, Action<ASTNode, ASTNode, Transformer>> transformMethods = new() {
            {"NumberLiteral", NumberLiteral },
            {"CallExpression", CallExpression },
            {"Operation", Operation }
        };

        private static void NumberLiteral(ASTNode node, ASTNode parent, Transformer program) {
            parent.Body.Add(new("NumberLiteral", node.Value));
        }

        private static void CallExpression(ASTNode node, ASTNode parent, Transformer program) {
            if (node.Value != "print") throw new Exception();

            walkNodes(node.Body, node, program);

            program.Includes.Add("msvcrt");
            program.Libs.Add("msvcrt");
            program.Prototypes.Add("printf proto c, :VARARG");
            program.StringLiterals.Add("fmt db \"%d\", 10, 0");

            ASTNode func = new("FunctionInvoke", "printf");
            func.Body.Add(new ASTNode("StringLiteral", "OFFSET fmt"));
            //func.Body.Add(new ASTNode("StringLiteral", "OFFSET "))
            func.Body.Add(new ASTNode("StringLiteral", "eax"));

            program.Program.Add(func);
        }

        private static void Operation(ASTNode node, ASTNode parent, Transformer program) {
            if (node.Body.Count != 2) throw new Exception();
            if (node.Body[0].Type != "NumberLiteral") throw new Exception();
            if (node.Body[1].Type != "NumberLiteral") throw new Exception();
            if (node.Value != "+") throw new Exception();

            program.Program.Add(new ASTNode("LoadRegister", node.Body[0].Value));
            program.Program.Add(new ASTNode("Add", node.Body[1].Value));
        }

    }
}
