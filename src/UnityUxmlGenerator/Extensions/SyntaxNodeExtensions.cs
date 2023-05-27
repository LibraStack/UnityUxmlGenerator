using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator.Extensions;

internal static class SyntaxNodeExtensions
{
    public static bool IsMemberHasAttribute<TMember>(this SyntaxNode syntaxNode, string attributeName,
        out (TMember Member, AttributeSyntax Attribute) result) where TMember : MemberDeclarationSyntax
    {
        if (syntaxNode is not TMember memberSyntax)
        {
            result = default;
            return false;
        }

        result.Member = memberSyntax;

        for (var i = 0; i < memberSyntax.AttributeLists.Count; i++)
        {
            var attributeList = memberSyntax.AttributeLists[i];
            for (var j = 0; j < attributeList.Attributes.Count; j++)
            {
                var attributeSyntax = attributeList.Attributes[j];
                switch (attributeSyntax.Name)
                {
                    case IdentifierNameSyntax identifierNameSyntax
                        when identifierNameSyntax.Identifier.Text.Contains(attributeName):
                    case QualifiedNameSyntax qualifiedNameSyntax
                        when qualifiedNameSyntax.Right.Identifier.Text.Contains(attributeName):
                    {
                        result.Attribute = attributeSyntax;
                        return true;
                    }
                }
            }
        }

        result = default;
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
        return GetTypeSymbol(syntaxNode, context)?.ContainingNamespace.ToString();
    }
}