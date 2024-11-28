using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler.Generation.Assembly
{
    internal class Function : IExportable
    {
        Statement[] Statements;
        string FunctionName;
        string ReturnType;

        // TODO for testing purposes
        public Function(string name, string returnType, params Statement[] statements)
        {
            this.FunctionName = name;
            this.Statements = statements;
            this.ReturnType = returnType;
        }


        public void Export(Stream stream, int indent)
        {
            Debug.Assert(indent == 0);

            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine($"{ReturnType} {FunctionName}()");
            writer.WriteLine("{");
            writer.Flush();
            foreach (Statement statement in this.Statements)
            {
                statement.Export(stream, 1);
            }
            writer.WriteLine("}");
            writer.Flush();
        }
    }
}
