using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler.Targets.C
{
    internal abstract class C_Compiler : ICompiler
    {
        private Dictionary<string, FunctionOptions> StdLib = new()
        {
            { "print", new FunctionOptions(){ ReturnType = new TypeName("void"), ArgumentTypes = new(){ new TypeName("uint8") } } }
        };

        public Dictionary<string, VariableOptions> KnownVariables => new();

        public Dictionary<string, FunctionOptions> KnownFunctions => StdLib;

        public void Compile(Parser program, StreamWriter outputFile)
        {
            throw new NotImplementedException();
        }
    }
}
