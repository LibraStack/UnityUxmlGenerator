using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator.Extensions;

internal static class TypeSyntaxExtensions
{
    public static bool IsBoolType(this TypeSyntax typeSyntax)
    {
        if (typeSyntax is PredefinedTypeSyntax predefinedTypeSyntax)
        {
            return IsBoolType(predefinedTypeSyntax);
        }

        return IsBoolKind(typeSyntax.RawKind);
    }

    public static bool IsBoolType(this PredefinedTypeSyntax typeSyntax)
    {
        return IsBoolKind(typeSyntax.Keyword.RawKind);
    }

    public static bool IsStringType(this TypeSyntax typeSyntax)
    {
        if (typeSyntax is PredefinedTypeSyntax predefinedTypeSyntax)
        {
            return IsStringType(predefinedTypeSyntax);
        }

        return IsStringKind(typeSyntax.RawKind);
    }

    public static bool IsStringType(this PredefinedTypeSyntax typeSyntax)
    {
        return IsStringKind(typeSyntax.Keyword.RawKind);
    }

    public static bool IsNumericType(this TypeSyntax typeSyntax)
    {
        if (typeSyntax is PredefinedTypeSyntax predefinedTypeSyntax)
        {
            return IsNumericType(predefinedTypeSyntax);
        }

        return IsNumericKind(typeSyntax.RawKind);
    }

    public static bool IsNumericType(this PredefinedTypeSyntax typeSyntax)
    {
        return IsNumericKind(typeSyntax.Keyword.RawKind);
    }

    private static bool IsBoolKind(int rawKind)
    {
        return rawKind == (int) SyntaxKind.BoolKeyword;
    }

    private static bool IsStringKind(int rawKind)
    {
        return rawKind == (int) SyntaxKind.StringKeyword;
    }

    private static bool IsNumericKind(int rawKind)
    {
        return rawKind == (int) SyntaxKind.IntKeyword ||
               rawKind == (int) SyntaxKind.LongKeyword ||
               rawKind == (int) SyntaxKind.FloatKeyword ||
               rawKind == (int) SyntaxKind.DoubleKeyword;
    }
}