using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bumblebee_Compiler
{
    public class CLI
    {
        string exe;
        string? workingDir;

        public CLI(string exeFile, string? WorkingDirectory = null)
        {
            this.exe = exeFile;
            this.workingDir = WorkingDirectory;
        }

        private Process StartProcess(params string[] args)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = exe;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            if (this.workingDir != null) {
                startInfo.WorkingDirectory = this.workingDir;
            }

            foreach (string arg in args)
            {
                startInfo.ArgumentList.Add(arg);
            }

            var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new Exception("Failed to start process.");
            }
            return process;
        }

        private void RunCommandInternal(params string[] args)
        {
            var process = StartProcess(args);

            char[] buffer = new char[100];
            while (true)
            {
                try
                {
                    if (process.StandardOutput.Peek() >= 0) {
                        Console.WriteLine(process.StandardOutput.ReadLine());
                    } else if(process.StandardError.Peek() >= 0) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(process.StandardError.ReadLine());
                        Console.ResetColor();
                    } else
                    {
                        if (process.HasExited)
                        {
                            break;
                        } else
                        {
                            //Thread.Sleep(100);
                        }
                    }
                }
                catch (Exception ex)
                {
                    process.Close();
                    throw new Exception("Failed to read input.", ex);
                }
            }

            try
            {
                process.WaitForExit();
            }
            catch (Exception)
            {
                process.Close();
            }

        }

        public void RunCommand(params string[] args)
        {
            /*List<string> result = new();
            foreach(string line in RunCommandInternal(args))
            {
                Console.WriteLine(line);
                result.Add(line);
            }
            return result;*/
            RunCommandInternal(args);
        }

        public void RunCommandSilent(params string[] args)
        {
            RunCommandInternal(args);
        }
    }
}
