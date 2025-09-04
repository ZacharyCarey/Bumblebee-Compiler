using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler.Generation.Assembly
{
    internal class AssemblyGeneration
    {

        List<IExportable> code = new();

        public AssemblyGeneration() { }

        public void Add(IExportable exportable)
        {
            code.Add(exportable);
        }

        public void Compile()
        {
            Console.WriteLine("Generating C project files...");
            Dictionary<string, string> CMakeArgs = new()
            {
                { "BUMBLEBEE_PROJECT_NAME", "ExampleC" },
                { "BUMBLEBEE_EXECUTABLE_SOURCE_FILE", "main.c" }
            };

            const string CompiledCodeFolder = "CompiledCode";
            string SrcFolder = Path.Combine(CompiledCodeFolder, "src");
            string BuildFolder = Path.Combine(CompiledCodeFolder, "build");

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
                foreach (var codeLine in code)
                {
                    codeLine.Export(stream, 0);
                }
            }

            Console.WriteLine("Compiling...");
            CLI cli = new CLI("cmake", BuildFolder);
            cli.RunCommand("-G", "MinGW Makefiles", Path.Combine("..", "src"));

            cli = new CLI("mingw32-make", BuildFolder);
            cli.RunCommand(/*"install"*/);
            Console.WriteLine("Done.");
        }

    }
}
