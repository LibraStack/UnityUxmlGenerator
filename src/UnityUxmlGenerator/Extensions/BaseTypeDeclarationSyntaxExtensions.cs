using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator.Extensions;

internal static class BaseTypeDeclarationSyntaxExtensions
{
    public static bool InheritsFromFullyQualifiedName(this BaseTypeDeclarationSyntax @class,
        GeneratorExecutionContext context, string name)
    {
        INamedTypeSymbol? symbol = context.Compilation.GetSemanticModel(@class.SyntaxTree).GetDeclaredSymbol(@class);

        if (symbol?.ToString() == name)
        {
            return true;
        }

        symbol = symbol?.BaseType;

        while (symbol != null)
        {
            if (symbol.ToString() == name)
            {
                return true;
            }

            symbol = symbol.BaseType;
        }

        return false;
    }
}