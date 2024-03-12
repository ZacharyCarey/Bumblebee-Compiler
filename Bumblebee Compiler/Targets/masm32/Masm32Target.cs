using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler.Targets.masm32
{
    internal class Masm32Target
    {

        private void todo()
        {
            //// Transform the original Bumblebee AST code into Assembly AST code
            //var transformer = new Transformer();
            //transformer.Transform(parser.AST);


            //// Generate assembly code using the Assembly AST code
            //// (And export to a file for the next step)
            //File.Delete("Program.asm");
            //using (StreamWriter writer = new(File.OpenWrite("Program.asm"))) {
            //    new Assembler(writer, transformer).Assemble();
            //}


            //// directory locations
            //var asmPath = Directory.GetCurrentDirectory() + "\\";
            //var asmName = "Program";

            //// Compile the assembly code into machine code object file
            //// ml.exe /c /Zd /coff Program.asm
            //Process p = new Process();
            //p.StartInfo.FileName = "masm32/bin/ml.exe";
            //p.StartInfo.Arguments = "/c /Zd /coff \"" + asmPath + asmName + ".asm\"";
            //p.StartInfo.WorkingDirectory = "masm32/bin";
            //p.Start();
            //p.WaitForExit();
            //if (p.ExitCode != 0) {
            //    // Check for extra files.
            //    //File.Delete("/masm32/bin/Program.obj");
            //    //File.Delete("/masm32/bin/Program.exe");
            //    return;
            //}

            //// Link the object file into an executable file by linking
            //// link /SUBSYSTEM:CONSOLE Program.obj
            //p = new Process();
            //p.StartInfo.FileName = "masm32/bin/link.exe";
            //p.StartInfo.Arguments = "/SUBSYSTEM:CONSOLE " + asmName + ".obj";
            //p.StartInfo.WorkingDirectory = "masm32/bin";
            //p.Start();
            //p.WaitForExit();
            //if (p.ExitCode != 0) {
            //    return;
            //}

            //// Run the code
            //Console.WriteLine("Running....");
            //p = new Process();
            //p.StartInfo.FileName = "masm32/bin/Program.exe";
            //p.StartInfo.WorkingDirectory = "masm32/bin";
            //p.Start();
            //p.WaitForExit();
            //Console.WriteLine("Program finished.");
        }

    }
}
