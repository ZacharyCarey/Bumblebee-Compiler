// See https://aka.ms/new-console-template for more information
using Bumblebee_Compiler;
using Bumblebee_Compiler.Targets.RISC_Z;
using Bumblebee_Compiler.Tokens;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;


string text = File.ReadAllText("TestPrograms/TuringCompleteDancingMachine.bee");
ICompiler compiler = new RISC_Z_Compiler();
const string outputFile = "TuringCompleteDancingMachine.asm";


// Read the text file into an array of objects with basic types
List<Token> tokens = new Tokenizer().ParseTokens(text).ToList();

//foreach(var token in tokens) {
//    Console.WriteLine(token);
//}

// Transform the tokens into an AST (array of objects) which represent the program
var parser = new Parser();
parser.SyntaxAnalyzer(tokens);

var typeChecker = new TypeChecker();
typeChecker.CheckTypes(parser, compiler);

File.Delete(outputFile);
using (StreamWriter writer = new(File.OpenWrite(outputFile))) {
    compiler.Compile(parser.AST, writer);
    writer.Flush();
}


