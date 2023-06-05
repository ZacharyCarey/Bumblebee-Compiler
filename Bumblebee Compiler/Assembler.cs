using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler {
    internal class Assembler {

        private StreamWriter writer;
        private Transformer program;

        internal Assembler(StreamWriter outputFile, Transformer program) {
            this.writer = outputFile;
            this.program = program;
        }

        internal void Assemble() {
            // Boiler plate
            writer.WriteLine("; Compiler settings");
            writer.WriteLine(".386");
            writer.WriteLine(".model flat, stdcall");
            writer.WriteLine("option casemap:none");

            writer.WriteLine();


            // "include \\masm32\\include\\{include}.inc"
            writer.WriteLine("; Include files");
            foreach(string include in program.Includes) {
                writer.WriteLine($"include ..\\include\\{include}.inc");
            }

            writer.WriteLine();

            writer.WriteLine("; Libs");
            foreach(string lib in program.Libs) {
                writer.WriteLine($"includelib ..\\lib\\{lib}.lib");
            }

            writer.WriteLine();

            writer.WriteLine("; Prototypes");
            foreach(string proto in program.Prototypes) {
                writer.WriteLine(proto);
            }

            writer.WriteLine();
            writer.WriteLine(".DATA");
            writer.WriteLine();

            foreach(string data in program.StringLiterals) {
                writer.WriteLine(data);
            }

            writer.WriteLine();
            writer.WriteLine(".CODE");
            writer.WriteLine();

            writer.WriteLine("main proc");

            foreach(ASTNode node in program.Program) {
                if (node.Type == "LoadRegister") {
                    writer.WriteLine($"mov eax, {node.Value}");
                } else if (node.Type == "Add") {
                    writer.WriteLine($"add eax, {node.Value}");
                } else if (node.Type == "FunctionInvoke") {
                    StringBuilder line = new();
                    line.Append("invoke ");
                    line.Append(node.Value);
                    foreach(ASTNode param in node.Body) {
                        if (param.Type != "StringLiteral") throw new Exception();
                        line.Append(", ");
                        line.Append(param.Value);
                    }
                    writer.WriteLine(line.ToString());
                } else {
                    throw new Exception();
                }
            }

            writer.WriteLine("invoke exit, 0");
            writer.WriteLine("main endp");
            writer.WriteLine("END main");

            writer.Flush();
        }

    }
}
