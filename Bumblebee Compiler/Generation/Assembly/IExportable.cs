using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler.Generation.Assembly
{
    internal interface IExportable
    {
        internal void Export(Stream stream, int indent);
    }
}
