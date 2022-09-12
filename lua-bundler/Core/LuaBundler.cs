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
        private string _workDir = ".";
        private string _distCode = "";
        //TODO: "", ' 패턴 추가하기
        private readonly Regex _regex = new Regex("require\\(\"([0-9\\/a-zA-Z_-]+)\"\\)");
        private readonly Dictionary<string, bool> _requires = new Dictionary<string, bool>();
        
        #region Lua Code Snippets
        private const string RemarkHeader = 
@"-- Bundled Files: {0}
-- Unused Files: {1}
-- Bundled At: {2}";

        private const string CodeHeader = 
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
        private const string CodeFooter = "\n-- Execute Main Function" +
                                          "\n__modules[\"{0}\"].loader()";
        #endregion
        
        #region Utility
        /// <summary xml:lang="ko">
        /// 확장자를 제외한 파일 이름을 Get합니다
        /// </summary>
        private static string GetFileName(string mainPath) => Path.GetFileNameWithoutExtension(mainPath);

        /// <summary xml:lang="ko">
        /// 디렉토리 내의 모든 루아 파일의 이름을 얻습니다.
        /// </summary>
        private static IEnumerable<string> GetAllLuaFileNames(string dir)
        {
            var names = new List<string>();
            var files = Directory.GetFiles($@"{dir}", "*.lua", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var name = file
                    .Replace($@"{dir}\", "")
                    .Replace(".lua", "")
                    .Replace(@"\", "/");
                names.Add(name);
            }
            return names;
        }
        
        /// <summary xml:lang="ko">
        /// 루아파일 시작라인 마다 탭 처리를 한 뒤 리턴합니다.
        /// </summary>
        private static string ReplaceWithPad(string file) 
            => "\t" + file.Replace(Environment.NewLine, "\n\t");
        
        /// <summary xml:lang="ko">
        /// 파일을 동기적으로 작성합니다
        /// </summary>
        private static void CreateFileSync(string outPath, string file)
        {
            using (var fs = File.Create(outPath))
            {
                var info = new UTF8Encoding(true).GetBytes(file);
                fs.Write(info,0,info.Length);
            }
        }

        /// <summary xml:lang="ko">
        /// 파일 여부 리턴
        /// </summary>
        private static bool CheckFileExist(string path)
        {
            var fi = new FileInfo(path).Exists;
            if (!fi)
                Logger.Error($"File not found - {path}");
            return fi;
        }
        #endregion
        
        #region Main Logic
        /// <summary xml:lang="ko">
        /// 폴더 내에 안쓰는 루아 모듈 얻기
        /// </summary>
        private List<string> GetUnusedFiles(string dir)
        {
            var list = new List<string>();
            var names = GetAllLuaFileNames(dir);
            foreach(var name in names)
            {
                if (!_requires.ContainsKey(name))
                {
                    list.Add(name);
                }
            }
            return list;
        }
        
        /// <summary xml:lang="ko">
        /// require 예약어 걸린 파일 이름 리스트 얻기
        /// </summary>
        private List<string> GetNewFileNames(string file)
        {
            var matches = _regex.Matches(file);
            
            // 중복 파일일 경우 Emit 하지 않기
            var newNames = new List<string>();
            foreach (Match match in matches)
            {
                var newName = match.Groups[1].ToString();
                if (_requires.ContainsKey(newName))
                {
                    _requires[newName] = false;
                }
                else
                {
                    newNames.Add(newName);
                }
            }

            return newNames;
        }

        /// <summary xml:lang="ko">
        /// 루아 코드를 재귀적으로 생성합니다
        /// </summary>
        /// <param name="name"></param>
        private void RecurseFiles(string name)
        {
            if (_requires.ContainsKey(name) && _requires[name])
                return;
            
            var filePath = Path.Combine(_workDir, name + ".lua");

            if (!CheckFileExist(filePath))
                return;

            var file = ReplaceWithPad(File.ReadAllText(filePath));
            
            _distCode += "\n----------------\n";
            _distCode += $"__modules[\"{name}\"] = " + "{ inited = false, cached = false, loader = function(...)";
            _distCode += $"\n---- START {name}.lua ----\n";
            _distCode += file;
            _distCode += $"\n---- END {name}.lua ----\n";
            _distCode += "end}";
            
            _requires[name] = true;
            
            // logger
            Logger.Success(filePath);
            
            // 뎁스 추적하며 require 예약어가 걸린 파일들 생성하기
            foreach (var newName in GetNewFileNames(file))
            {
                RecurseFiles(newName);
            }
        }
        
        /// <summary xml:lang="ko">
        /// 전체 코드 생성기
        /// </summary>
        private string GenerateCode(string mainPath)
        {
            var fi = new FileInfo(mainPath);
            
            if (!CheckFileExist(mainPath))
                return "";

            _workDir = fi.DirectoryName;
            
            _distCode = CodeHeader;

            var mainFunctionName = GetFileName(mainPath);

            RecurseFiles(mainFunctionName);
            
            var unusedFiles = GetUnusedFiles(_workDir);
            
            foreach (var unusedFile in unusedFiles)
            {
                Logger.Warn($"Unused File: {unusedFile}");
            }
            
            // 결과물 출력
            return $"{string.Format(RemarkHeader, _requires.Count, unusedFiles.Count, DateTime.Now.ToString(CultureInfo.InvariantCulture))}\n" +
                   $"{_distCode}" +
                   $"{string.Format(CodeFooter, mainFunctionName)}";
        }
        #endregion
        
        #region Public Methods
        /// <summary xml:lang="ko">
        /// 루아 파일들을 하나로 묶어서 번들링 해줍니다.
        /// </summary>
        public void ToFile(string mainPath, string outPath)
        {
            if (!CheckFileExist(mainPath))
                return;

            var luaFile = GenerateCode(mainPath);
            
            CreateFileSync(outPath, luaFile);
            
            Logger.Success($"Bundled Files: {_requires.Count}, Unused Files: {GetUnusedFiles(_workDir).Count}");
            _requires.Clear();
        }
        #endregion
    }
}