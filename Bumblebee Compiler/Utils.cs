using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler
{
    public static class Utils
    {
        public static string? SearchLocalExeFiles(string relativeExePath)
        {
            try
            {
                string cd = Assembly.GetEntryAssembly().Location;
                string parentFolder = Path.GetDirectoryName(cd);
                string path = Path.Combine(parentFolder, relativeExePath);
                if (File.Exists(path))
                {
                    return path;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
