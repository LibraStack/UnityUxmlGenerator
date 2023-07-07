using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator.Extensions;

internal static class SyntaxNodeExtensions
{
    public static bool IsAttributeWithName(this SyntaxNode syntaxNode, string attributeName,
        out AttributeSyntax? attribute)
    {
        if (syntaxNode is not AttributeSyntax attributeSyntax)
        {
            attribute = default;
            return false;
        }

        attribute = attributeSyntax;

        switch (attributeSyntax.Name)
        {
            case IdentifierNameSyntax identifierNameSyntax
                when identifierNameSyntax.Identifier.Text.Contains(attributeName):
            case QualifiedNameSyntax qualifiedNameSyntax
                when qualifiedNameSyntax.Right.Identifier.Text.Contains(attributeName):
                return true;
        }

        return false;
    }

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

    public static ITypeSymbol? GetTypeSymbol(this SyntaxNode syntaxNode, GeneratorExecutionContext context)
    {
        return context.Compilation.GetSemanticModel(syntaxNode.SyntaxTree).GetTypeInfo(syntaxNode).Type;
    }

    public static string? GetTypeNamespace(this SyntaxNode syntaxNode, GeneratorExecutionContext context)
    {
        var containingNamespace = GetTypeSymbol(syntaxNode, context)?.ContainingNamespace;

        if (containingNamespace is null || containingNamespace.IsGlobalNamespace)
        {
            return null;
        }

        return containingNamespace.ToString();
    }
}