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
        #region Lua Code Snippets
        private const string remarkHeader = 
@"-- Bundled Files: {0}
-- Unused Files: {1}
-- Bundled At: {2}";

        private const string codeHeader = 
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
        private const string codeFooter = "\n__modules[\"{0}\"].loader()";
        #endregion
        
        private string workDir = ".";
        private string outStr = "";
        private Regex regex = new Regex("require\\(\"([0-9\\/a-zA-Z_-]+)\"\\)");
        private Dictionary<string, bool> requireList = new Dictionary<string, bool>();
        
        /// <summary xml:lang="ko">
        /// 루아 코드를 재귀적으로 생성합니다
        /// </summary>
        /// <param name="name"></param>
        private void RecurseFiles(string name)
        {
            // 이미 생성했던 파일이면 return
            if (requireList.ContainsKey(name) && requireList[name])
                return;
            
            var fpath = Path.Combine(workDir, name + ".lua");
            
            var fileInfo = new FileInfo(fpath);

            if (!fileInfo.Exists)
            {
                Logger.Error($"File not found - {fpath}");
                return;
            }

            var file = File.ReadAllText(fpath);
            var matches = regex.Matches(file);
            
            outStr += "\n----------------\n";
            outStr += $"__modules[\"{name}\"] = " + "{ inited = false, cached = false, loader = function(...)";
            outStr += $"\n---- START {name}.lua ----\n";
            outStr += file;
            outStr += $"\n---- END {name}.lua ----\n";
            outStr += " end}";
            requireList[name] = true;
            
            // logger
            Logger.Success(fpath);
            
            // require 예약어 처리
            // 중복 파일일 경우 Emit 하지 않기
            var newNames = new List<string>();
            foreach (Match match in matches)
            {
                var newName = match.Groups[1].ToString();
                if (requireList.ContainsKey(newName))
                {
                    requireList[newName] = false;
                }
                else
                {
                    newNames.Add(newName);
                }
            }

            // 뎁스 추적하며 파일 생성하기
            foreach (var newName in newNames)
            {
                RecurseFiles(newName);
            }
        }
        
        private List<string> GetUnusedFiles(string dir)
        {
            var list = new List<string>();
            var files = Directory.GetFiles($@"{dir}", "*.lua", SearchOption.AllDirectories);
            foreach(var file in files)
            {
                var name = file
                    .Replace($@"{dir}\", "")
                    .Replace(".lua", "")
                    .Replace(@"\", "/");
                    
                if (!requireList.ContainsKey(name))
                {
                    list.Add(name);
                }
            }

            return list;
        }

        /// <summary xml:lang="ko">
        /// 확장자를 제외한 파일 이름을 Get합니다
        /// </summary>
        private string GetFileName(string mainPath) => Path.GetFileNameWithoutExtension(mainPath);

        /// <summary>
        /// 코드 내보내기
        /// </summary>
        private string EmitCode(string mainPath)
        {
            var file = new FileInfo(mainPath);

            if (!file.Exists)
            {
                Logger.Error($"File not found - {mainPath}");
                return "";
            }

            workDir = file.DirectoryName;
            
            outStr = codeHeader;

            var mainFunctionName = GetFileName(mainPath);

            RecurseFiles(mainFunctionName);
            
            var unusedFiles = GetUnusedFiles(workDir);
            
            foreach (var unusedFile in unusedFiles)
            {
                Logger.Warn($"Unused File: {unusedFile}");
            }
            
            // 결과물 출력
            return $"{string.Format(remarkHeader, requireList.Count, unusedFiles.Count, DateTime.Now.ToString(CultureInfo.InvariantCulture))}\n" +
                   $"{outStr}" +
                   $"{string.Format(codeFooter, mainFunctionName)}";
        }

        /// <summary xml:lang="ko">
        /// 파일을 동기적으로 작성합니다
        /// </summary>
        private void CreateFileSync(string outPath, string file)
        {
            using (var fs = File.Create(outPath))
            {
                var info = new UTF8Encoding(true).GetBytes(file);
                fs.Write(info,0,info.Length);
            }
        }

        /// <summary xml:lang="ko">
        /// 루아 파일들을 하나로 묶어서 번들링 해줍니다.
        /// </summary>
        public void ToFile(string mainPath, string outPath)
        {
            var fPath = new FileInfo(mainPath);
            
            if (!fPath.Exists)
            {
                Logger.Error($"File not found - {mainPath}");
                return;
            }

            var luaFile = EmitCode(mainPath);
            
            CreateFileSync(outPath, luaFile);
            
            Logger.Success($"Bundled Files: {requireList.Count}, Unused Files: {GetUnusedFiles(workDir).Count}");
            requireList.Clear();
        }
    }
}