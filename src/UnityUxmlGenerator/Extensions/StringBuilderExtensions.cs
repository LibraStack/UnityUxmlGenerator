using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator.Extensions;

public static class StringBuilderExtensions
{
    public static void AppendGenericString(this StringBuilder stringBuilder, GeneratorExecutionContext context,
        IEnumerable<TypeSyntax> genericTypeArguments)
    {
        var isFirstArgument = true;

        stringBuilder.Append('<');

        foreach (var genericClassTypeSyntax in genericTypeArguments)
        {
            if (isFirstArgument == false)
            {
                stringBuilder.Append(", ");
            }

            isFirstArgument = false;

            switch (genericClassTypeSyntax)
            {
                case PredefinedTypeSyntax predefinedTypeSyntax:
                    AppendPredefinedTypeSyntax(stringBuilder, predefinedTypeSyntax);
                    break;

                case IdentifierNameSyntax identifierNameSyntax:
                    AppendIdentifierNameSyntax(stringBuilder, context, identifierNameSyntax);
                    break;

                case GenericNameSyntax genericTypeSyntax:
                    AppendGenericTypeSyntax(stringBuilder, context, genericTypeSyntax);
                    break;
            }
        }

        stringBuilder.Append('>');
    }

    private static void AppendPredefinedTypeSyntax(StringBuilder stringBuilder,
        PredefinedTypeSyntax predefinedTypeSyntax)
    {
        stringBuilder.Append(predefinedTypeSyntax.Keyword.Text);
    }

    private static void AppendIdentifierNameSyntax(StringBuilder stringBuilder, GeneratorExecutionContext context,
        IdentifierNameSyntax identifierNameSyntax)
    {
        var genericClassName = identifierNameSyntax.Identifier.Text;
        var genericClassNamespace = identifierNameSyntax.GetTypeNamespace(context);

        stringBuilder.Append("global::");
        stringBuilder.Append(genericClassNamespace);
        stringBuilder.Append('.');
        stringBuilder.Append(genericClassName);
    }

    private static void AppendGenericTypeSyntax(StringBuilder stringBuilder, GeneratorExecutionContext context,
        GenericNameSyntax genericTypeSyntax)
    {
        var genericClassName = genericTypeSyntax.Identifier.Text;
        var genericClassNamespace = genericTypeSyntax.GetTypeNamespace(context);

        if (string.IsNullOrWhiteSpace(genericClassNamespace) == false)
        {
            stringBuilder.Append("global::");
            stringBuilder.Append(genericClassNamespace);
            stringBuilder.Append('.');
        }

        stringBuilder.Append(genericClassName);

        AppendGenericString(stringBuilder, context, genericTypeSyntax.TypeArgumentList.Arguments);
    }
}