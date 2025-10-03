using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.IO;

namespace STAMP.Lib
{
    internal static class FilesHelper
    {
        public static List<SyntaxTree> GetTrees(string path)
        {
            var result = new List<SyntaxTree>();

            FillTrees(new DirectoryInfo(path), result);

            return result;
        }

        private static void FillTrees(DirectoryInfo directory, List<SyntaxTree> trees)
        {
            var files = directory.GetFiles("*.cs");
            foreach (var file in files)
            {
                using (StreamReader reader = new StreamReader(file.OpenRead()))
                {
                    trees.Add(CSharpSyntaxTree.ParseText(reader.ReadToEnd()));
                }
            }

            var directories = directory.GetDirectories();
            foreach (var d in directories)
            {
                FillTrees(d, trees);
            }
        }
    }
}
