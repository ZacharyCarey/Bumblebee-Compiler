// See https://aka.ms/new-console-template for more information


using Bumblebee_Compiler;
using Bumblebee_Compiler.Generation.Assembly;
using Bumblebee_Compiler.Tokens;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

Console.WriteLine("Parsing");
string text = File.ReadAllText("TestPrograms/RandomNumberGenerator.bee");
//ICompiler compiler = new RISC_Z_Compiler();
//const string outputFile = "TuringCompleteRobotRacing.asm";


// Read the text file into an array of objects with basic types
List<Token> tokens = new Tokenizer().ParseTokens(text).ToList();

foreach(var token in tokens) {
    Console.WriteLine(token);
}

//// Transform the tokens into an AST (array of objects) which represent the program
//var parser = new Parser();
//parser.SyntaxAnalyzer(tokens);

//var typeChecker = new TypeChecker();
//typeChecker.CheckTypes(parser, compiler);

//Directory.CreateDirectory("output");
//File.Delete("output/"+outputFile);
//using (StreamWriter writer = new(File.OpenWrite("output/" + outputFile))) {
//    compiler.Compile(parser, writer);
//    writer.Flush();
//}
return;

Console.WriteLine("Parsing code...");
AssemblyGeneration compiler = new(); ;

compiler.Add(new Statement("#include <stdio.h>"));
compiler.Add(new Statement("#define PRINT_STR \"Hello, world!\""));
compiler.Add(new Function("main", "int", 
    new Statement("printf(PRINT_STR);"),
    new Statement("return 0;")
));


compiler.Compile();
