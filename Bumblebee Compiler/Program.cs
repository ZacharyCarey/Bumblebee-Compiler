// See https://aka.ms/new-console-template for more information
//using Bumblebee_Compiler;
//using Bumblebee_Compiler.Generation.Assembly;
//using Bumblebee_Compiler.Targets.RISC_Z;
//using System.Diagnostics;
//using System.Runtime.CompilerServices;
//using System.Text.RegularExpressions;


//string text = File.ReadAllText("TestPrograms/TuringCompleteRobotRacing.bee");
//ICompiler compiler = new RISC_Z_Compiler();
//const string outputFile = "TuringCompleteRobotRacing.asm";


//// Read the text file into an array of objects with basic types
//List<Token> tokens = new Tokenizer().ParseTokens(text).ToList();

////foreach(var token in tokens) {
////    Console.WriteLine(token);
////}

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

using Bumblebee_Compiler;
using Bumblebee_Compiler.Generation.Assembly;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

Console.WriteLine("Parsing code...");
List<IExportable> Code = new();

Code.Add(new Statement("#include <stdio.h>"));
Code.Add(new Statement("#define PRINT_STR \"Hello, world!\""));
Code.Add(new Function("main", "int", 
    new Statement("printf(PRINT_STR);"),
    new Statement("return 0;")
));
Console.WriteLine("Code parsed.");

Dictionary<string, string> CMakeArgs = new()
{
    { "BUMBLEBEE_PROJECT_NAME", "ExampleC" },
    { "BUMBLEBEE_EXECUTABLE_SOURCE_FILE", "main.c" }
};

Console.WriteLine("Generating C project files...");
const string CompiledCodeFolder = "CompiledCode";
string SrcFolder = Path.Combine(CompiledCodeFolder, "src");
string BuildFolder = Path.Combine(CompiledCodeFolder, "build");

// Compile code
if (Directory.Exists(CompiledCodeFolder))
{
    Directory.Delete(CompiledCodeFolder, true);
}

Directory.CreateDirectory(CompiledCodeFolder);
Directory.CreateDirectory(SrcFolder);
Directory.CreateDirectory(BuildFolder);

string? baseCmakePath = Utils.SearchLocalExeFiles(Path.Combine("Generation", "Assembly", "BaseCMakeLists.txt"));

// TODO assume it was successful for testing
string CMake = File.ReadAllText(baseCmakePath);
foreach (var pair in CMakeArgs)
{
    CMake = CMake.Replace($"${{{pair.Key}}}", pair.Value);
}

File.WriteAllText(Path.Combine(SrcFolder, "CMakeLists.txt"), CMake);

using (Stream stream = File.Create(Path.Combine(SrcFolder, "main.c")))
{
    foreach (var codeLine in Code)
    {
        codeLine.Export(stream, 0);
    }
}
Console.WriteLine("Files generated.");

Console.WriteLine("Compiling...");
CLI cli = new CLI("cmake", BuildFolder);
cli.RunCommand("-G", "MinGW Makefiles", Path.Combine("..", "src"));

cli = new CLI("mingw32-make", BuildFolder);
cli.RunCommand(/*"install"*/);
Console.WriteLine("Compiled.");


//var inputAsString = Encoding.ASCII.GetString(stream.ToArray());
//Console.WriteLine(inputAsString);
