using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler {
    public struct VariableOptions {
        public bool IsReadable;
        public bool IsWritable;
        public string TypeName;
    }

    public struct FunctionOptions {
        public string ReturnType;
        public List<string> ArgumentTypes = new();

        public FunctionOptions() { }
    }

    public interface ICompiler {

        public Dictionary<string, VariableOptions> KnownVariables { get; }
        public Dictionary<string, FunctionOptions> KnownFunctions { get; }
        public void Compile(Parser program, StreamWriter outputFile);

    }
}
