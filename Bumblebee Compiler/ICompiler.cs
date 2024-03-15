using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler {
    public struct VariableOptions {
        public bool IsReadable;
        public bool IsWritable;
        public TypeName TypeName;
    }

    public struct FunctionOptions {
        public TypeName ReturnType;
        public List<TypeName> ArgumentTypes = new();

        public FunctionOptions() { }
    }

    public interface ICompiler {

        public Dictionary<string, VariableOptions> KnownVariables { get; }
        public Dictionary<string, FunctionOptions> KnownFunctions { get; }
        public void Compile(Parser program, StreamWriter outputFile);

    }
}
