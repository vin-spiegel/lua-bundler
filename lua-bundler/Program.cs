using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using lua_bundler.Core;

namespace lua_bundler
{
    internal class Program : Process
    {
        private FileGenerator _generator;
        private const string baseSourcePath = @"C:\Projects\lua-bundler\lua-bundler\Example\src\";
        private const string baseDistPath = @"C:\Projects\lua-bundler\lua-bundler\Example\dist\";
        
        public static void Main(string[] args)
        {
            var a = new Program();
        }

        public Program()
        {
            _generator = new FileGenerator();
            Executor();
        }

        private bool inited;
        
        private void Executor()
        {
            while (true)
            {
                if (!inited)
                {
                    Console.WriteLine("---------------------------------------");
                    Console.WriteLine("번들링 파일과 출력 파일 경로를 적어주세요");
                    Console.WriteLine("ex) \"input.lua\" \"output.lua\"");
                    inited = true;
                }

                var cmd = Console.ReadLine();
                if (cmd == "exit" || cmd == "q")
                {
                    break;
                }
                
                var args = cmd.Split(' ');
                if (args.Length == 2)
                {
                    _generator.ToFile($@"{baseSourcePath}{args[0]}",$@"{baseDistPath}{args[1]}");
                }
            }
        }
    }
}