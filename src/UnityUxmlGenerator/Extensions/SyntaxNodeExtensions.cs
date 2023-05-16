using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator.Extensions;

internal static class SyntaxNodeExtensions
{
    public static T? GetParent<T>(this SyntaxNode syntaxNode)
    {
        var parent = syntaxNode.Parent;

        while (parent != null)
        {
            if (parent is T result)
            {
                return result;
            }

            parent = parent.Parent;
        }

        return default;
    }

    public static string? GetTypeNamespace(this TypeSyntax typeSyntax, GeneratorExecutionContext context)
    {
        return context.Compilation
            .GetSemanticModel(typeSyntax.SyntaxTree)
            .GetTypeInfo(typeSyntax).Type?.ContainingNamespace.ToString();
    }
}