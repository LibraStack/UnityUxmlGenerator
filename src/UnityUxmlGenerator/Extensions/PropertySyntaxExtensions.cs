using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator.Extensions;

internal static class PropertySyntaxExtensions
{
    public static string GetName(this PropertyDeclarationSyntax property)
    {
        return property.Identifier.Text;
    }
}