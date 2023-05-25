// See https://aka.ms/new-console-template for more information
using Bumblebee_Compiler;
using Bumblebee_Compiler.Tokens;
using System.Diagnostics;
using System.Runtime.CompilerServices;

Compiler program = new Compiler();
using (StreamReader reader = new(File.OpenRead("Program.bee"))) {
    IEnumerable<Token> tokens = new Tokenizer(reader).ParseTokens();
    program.Compile(tokens);
}

using (StreamWriter writer = new(File.OpenWrite("Program.asm"))) {
    new Assembler(writer, program).Assemble();
}

var asmPath = Directory.GetCurrentDirectory() + "\\";
var asmName = "Program";

// ml.exe /c /Zd /coff Program.asm
Process p = new Process();
p.StartInfo.FileName = "masm32/bin/ml.exe";
p.StartInfo.Arguments = "/c /Zd /coff \"" + asmPath + asmName + ".asm\"";
p.StartInfo.WorkingDirectory = "masm32/bin";
p.Start();
p.WaitForExit();
if (p.ExitCode != 0) {
    // Check for extra files.
    //File.Delete("/masm32/bin/Program.obj");
    //File.Delete("/masm32/bin/Program.exe");
    return;
}

// link /SUBSYSTEM:CONSOLE Program.obj
p = new Process();
p.StartInfo.FileName = "masm32/bin/link.exe";
p.StartInfo.Arguments = "/SUBSYSTEM:CONSOLE " + asmName + ".obj";
p.StartInfo.WorkingDirectory = "masm32/bin";
p.Start();
p.WaitForExit();
if (p.ExitCode != 0) {
    return;
}

Console.WriteLine("Running....");
p = new Process();
p.StartInfo.FileName = "masm32/bin/Program.exe";
p.StartInfo.WorkingDirectory = "masm32/bin";
p.Start();
p.WaitForExit();
Console.WriteLine("Program finished.");

