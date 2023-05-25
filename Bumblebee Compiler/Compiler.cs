using Bumblebee_Compiler.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler {
    internal class Compiler {

        internal HashSet<string> Includes = new() {
            "windows",
            "user32",
            "kernel32",
            "gdi32",
        };

        internal HashSet<string> Libs = new() {
            "kernel32",
            "user32",
            "gdi32"
        };

        internal HashSet<string> Prototypes = new() {
            "exit proto c, :DWORD"
        };

        internal HashSet<string> Data = new();

        internal List<string> Code = new();

        private static readonly Dictionary<string, string[]> FunctionIncludes = new() {
            { "printf", new string[]{ "msvcrt" } }
        };

        private static readonly Dictionary<string, string[]> FunctionLibs = new() {
            { "printf", new string[]{ "msvcrt" } }
        };

        private static readonly Dictionary<string, string[]> FunctionPrototypes = new() {
            { "printf", new string[]{ "printf proto c, :VARARG" } }
        };

        private static readonly Dictionary<string, string[]> FunctionData = new() {
            { "printf", new string[]{ "fmt db \"%d\", 10, 0" } }
        };

        internal void Compile(IEnumerable<Token> tokens) {
            // Setup main func
            Code.Add("main proc");

            // Parse tokens
            IEnumerator<Token> iter = tokens.GetEnumerator();
            while (iter.MoveNext()) {
                ParseToken(iter);
            }

            // Close main func
            Code.Add("invoke exit, 0");
            Code.Add("main endp");
            Code.Add("END main");
        }

        private void ParseToken(IEnumerator<Token> iter) {
            if (iter.Current is LiteralToken literal) {
                if (literal.Value != "print") throw new Exception("Bad literal.");
                AddRequirements("printf");
                
                if (!iter.MoveNext()) throw new Exception("Expected argument token.");
                if (iter.Current is NumberToken arg) {
                    Code.Add($"invoke printf, OFFSET fmt, {arg.Value}, eax");
                } else {
                    throw new Exception("Unexpected arg type.");
                }

            } else {
                throw new Exception("Unexpected token.");
            }
        }

        private void AddRequirements(string name) {
            foreach(string include in FunctionIncludes[name]) {
                Includes.Add(include);
            }
            foreach(string lib in FunctionLibs[name]) {
                Libs.Add(lib);
            }
            foreach(string prototype in FunctionPrototypes[name]) {
                Prototypes.Add(prototype);
            }
            foreach(string data in FunctionData[name]) {
                Data.Add(data);
            }
        }

    }
}
