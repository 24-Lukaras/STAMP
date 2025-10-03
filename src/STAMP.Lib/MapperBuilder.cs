using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STAMP.Lib
{
    public class MapperBuilder
    {

        private readonly SyntaxTree _syntaxTree;
        private readonly ICompilationProvider _compilationProvider;
        public MapperBuilder(string content, ICompilationProvider compilationProvider)
        {
            _syntaxTree = CSharpSyntaxTree.ParseText(content);
            _compilationProvider = compilationProvider;
        }

        public async Task<string> Process()
        {
            var root = await _syntaxTree.GetRootAsync();

            var tupleAliases = root.DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Where(MapperMethodDefinition.IsUsingDirectiveSuitable)
                .ToList();

            var compilation = _compilationProvider.GetCompilationWith(new List<SyntaxTree>() { _syntaxTree });
            var model = compilation.GetSemanticModel(_syntaxTree);

            var methodDefinitions = tupleAliases.Select(x => new MapperMethodDefinition(x, model, compilation)).ToList();

            var newRoot = root.RemoveNodes(tupleAliases, SyntaxRemoveOptions.KeepNoTrivia);

            var mapperClass = newRoot.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();

            var modifiedClass = mapperClass.AddMembers(methodDefinitions.Select(x => x.CreateMethod()).ToArray());
            if (modifiedClass.Modifiers.Any(x => !x.IsKind(SyntaxKind.StaticKeyword)))
            {
                modifiedClass = modifiedClass.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
            }

            newRoot = newRoot.ReplaceNode(mapperClass, modifiedClass);

            newRoot = RegisterRequiredNamespaces(newRoot, methodDefinitions);

            newRoot = newRoot.NormalizeWhitespace();

            var result = SyntaxFactory.SyntaxTree(newRoot);
            return result.ToString();
        }

        private SyntaxNode RegisterRequiredNamespaces(SyntaxNode root, List<MapperMethodDefinition> mapperDefinitions)
        {
            var usings = root.DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Where(x => x.Alias is null);

            var usingNamespaces = usings.Select(x => x.NamespaceOrType.ToString())
                .ToList();

            var firstClass = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First();

            List<UsingDirectiveSyntax> usingsToAdd = new List<UsingDirectiveSyntax>();
            usingsToAdd.AddRange(usings);
            foreach (var mapperDefinition in mapperDefinitions)
            {
                foreach (var requiredNamespace in mapperDefinition.RequiredNamespaces)
                {
                    if (!usingNamespaces.Contains(requiredNamespace))
                    {
                        usingsToAdd.Add(
                            SyntaxFactory.UsingDirective(
                                SyntaxFactory.Token(SyntaxKind.UsingKeyword),
                                SyntaxFactory.Token(SyntaxKind.None),
                                null,
                                SyntaxFactory.IdentifierName(requiredNamespace),
                                SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        );
                        usingNamespaces.Add(requiredNamespace);
                    }
                }
            }

            if (usingsToAdd.Count == 0)
                return root;

            return (root as CompilationUnitSyntax).WithUsings(SyntaxFactory.List(usingsToAdd.OrderBy(x => x.NamespaceOrType.ToString())));
        }
    }
}
