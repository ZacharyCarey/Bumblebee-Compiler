using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler {
    internal class Assembler {

        private StreamWriter writer;
        private Compiler program;

        internal Assembler(StreamWriter outputFile, Compiler program) {
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


            writer.WriteLine("; Include files");
            foreach(string include in program.Includes) {
                writer.WriteLine($"include \\masm32\\include\\{include}.inc");
            }

            writer.WriteLine();

            writer.WriteLine("; Libs");
            foreach(string lib in program.Libs) {
                writer.WriteLine($"includelib \\masm32\\lib\\{lib}.lib");
            }

            writer.WriteLine();

            writer.WriteLine("; Prototypes");
            foreach(string proto in program.Prototypes) {
                writer.WriteLine(proto);
            }

            writer.WriteLine();
            writer.WriteLine(".DATA");
            writer.WriteLine();

            foreach(string data in program.Data) {
                writer.WriteLine(data);
            }

            writer.WriteLine();
            writer.WriteLine(".CODE");
            writer.WriteLine();

            foreach(string code in program.Code) {
                writer.WriteLine(code);
            }
        }

    }
}
