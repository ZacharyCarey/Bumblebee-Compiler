using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler.Generation.Assembly
{
    internal class Statement : IExportable
    {
        string GeneratedCode;

        // TODO testing purposes
        public Statement(string C_Code)
        {
            this.GeneratedCode = C_Code;
        }

        public void Export(Stream stream, int indent)
        {
            StreamWriter writer = new StreamWriter(stream);
            for(int i = 0; i < indent; i++)
            {
                writer.Write('\t');
            }
            writer.WriteLine(this.GeneratedCode);
            writer.Flush();
        }

    }
}
