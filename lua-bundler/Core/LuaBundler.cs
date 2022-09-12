using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace lua_bundler.Core
{
    public class FileGenerator
    {
        /// <summary>
        /// 번들 파일이름
        /// </summary>
        public string DistName => "___bundle___.lua";
        
        public string workDir = ".";
        public string outStr = "";
        /// <summary>
        /// 번들 파일의 헤더 부분
        /// </summary>
        public string codeHeader 
        {
            get
            {
                var nowDate = $"-- Bundled At : {DateTime.Now.ToString(CultureInfo.InvariantCulture)}\n";
                return nowDate + 
@"local __modules = {}
local require = function(path)
    local module = __modules[path]
    if module ~= nil then
        if not module.inited then
            module.cached = module.loader()
            module.inited = true
        end
        return module.cached
    else
        error('module not found')
        return nil
    end
end";
            }
        }
        private Regex regex = new Regex("^(?!--)(.*require)[ \t]*\\?(.*)");
        public string DistFile
        {
            get
            {
                var res = codeHeader + "";
                return res;
            }
        }
        
        private Dictionary<string, bool> requireList = new Dictionary<string, bool>();
        // private string outStr = "";
        
        public void recurseFiles(string name)
        {
            if (requireList.ContainsKey(name))
                return;
            
            var fpath = Path.Combine(workDir, name + ".lua");
            
            var file = new FileInfo(fpath);

            if (!file.Exists)
            {
                Console.WriteLine($@"File not found {fpath}");
                return;
            }

            var newNames = new List<string>();
            // const newNames = []

            // const file = fs.readFileSync(fpath).toString().replace(regex, (m, p1, p2, p3) =>
            // {
            //     return `${
            //         p1
            //     }
            //     ("${p2.replace(/\./g, " / ")}")${
            //         p3
            //     }`
            // });
            // outStr += "\n----------------\n";
            // outStr += $"__modules['{name}'] = " + "{ inited = false, cached = false, loader = function(...)";
            // outStr += $"\n---- START {name}.lua ----\n";
            // outStr += file;
            // outStr += $"\n---- END {name}.lua ----\n";
            // outStr += " end}";
            // requireList[name] = true;
            // requires
            // let match
            // do {
            //     match = regex.exec(file)
            //     if (match != null) {
            //         if (requireList[match[2]] === undefined) {
            //             requireList[match[2]] = false
            //             newNames.push(match[2])
            //         }
            //     }
            // } 
            // while (match != null)
            // {
            //     for (let i = 0; i < newNames.length; i++) 
            //     {
            //         recurseFiles(newNames[i])
            //     }
            // }
        }
        public string emitCode(string mainPath)
        {
            var file = new FileInfo(mainPath);

            if (!file.Exists)
            {
                Console.WriteLine($@"File not found {mainPath}");
                return "";
            }

            workDir = file.DirectoryName;

            outStr = codeHeader;

            var mainName = Path.GetFileNameWithoutExtension(mainPath);

            recurseFiles(mainName);
            // var unused = new List<string>();
            // findUnusedFiles(workDir, unused)
            // for (const file of unused)
            // {
            //     logger.warn("Unused file " + file + ".lua")
            // }
            return outStr + $"\n__modules[\"{mainName}\"].loader()";
        }

        /// <summary xml:lang="ko">
        /// 파일을 동기적으로 작성합니다
        /// </summary>
        public void CreateFileSync(string outPath, string file)
        {
            using (var fs = File.Create(outPath))
            {
                var info = new UTF8Encoding(true).GetBytes(file);
                fs.Write(info,0,info.Length);
            }
        }

        public void ToFile(string mainPath, string outPath)
        {
            var fPath = new FileInfo(mainPath);

            if (!fPath.Exists)
            {
                Console.WriteLine($@"File not found {mainPath}");
                return;
            }

            var file = emitCode(mainPath);
            CreateFileSync(outPath, file);
            
            requireList.Clear();
        }
    }
}