using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using lua_bundler.Core;

namespace lua_bundler
{
    internal class Program : Process
    {
        private readonly FileGenerator _bundler;
        private bool _inited;
        private const string baseSourcePath = @"C:\Projects\lua-bundler\lua-bundler\Example\src\";
        private const string baseDistPath = @"C:\Projects\lua-bundler\lua-bundler\Example\dist\";
        
        public static void Main(string[] args)
        {
            var a = new Program();
        }

        private Program()
        {
            _bundler = new FileGenerator();
            Executor();
        }

        
        private void Executor()
        {
            while (true)
            {
                if (!_inited)
                {
                    Console.WriteLine("---------------------------------------");
                    Console.WriteLine("번들링 파일과 출력 파일 경로를 적어주세요");
                    Console.WriteLine("ex) \"input.lua\" \"output.lua\"");
                    _inited = true;
                }

                var cmd = Console.ReadLine();
                if (cmd == "exit" || cmd == "q")
                {
                    break;
                }
                
                var args = cmd.Split(' ');
                if (args.Length == 2)
                {
                    _bundler.ToFile($@"{baseSourcePath}{args[0]}",$@"{baseDistPath}{args[1]}");
                }
            }
        }
    }
}