using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityUxmlGenerator.Captures;
using UnityUxmlGenerator.Extensions;

namespace UnityUxmlGenerator.SyntaxReceivers;

internal sealed class UxmlTraitsReceiver : ISyntaxReceiver
{
    private const string AttributeName = "UxmlAttribute";

    private readonly Dictionary<string, UxmlTraitsCapture> _captures = new();

    public IReadOnlyDictionary<string, UxmlTraitsCapture> Captures => _captures;

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not AttributeSyntax
            {
                Name: IdentifierNameSyntax { Identifier.Text: AttributeName }
            } attribute)
        {
            return;
        }

        var property = attribute.GetParent<PropertyDeclarationSyntax>();
        if (property is null)
        {
            return;
        }

        if (attribute.ArgumentList is not null && attribute.ArgumentList.Arguments.Any())
        {
            if (HasSameType(property, attribute) == false)
            {
                return;
            }
        }

        var @class = property.GetParent<ClassDeclarationSyntax>();
        if (@class?.BaseList is null || @class.BaseList.Types.Count == 0)
        {
            return;
        }

        if (_captures.TryGetValue(@class.Identifier.Text, out var uxmlTraits) == false)
        {
            uxmlTraits = new UxmlTraitsCapture(@class, @class.BaseList.Types.First().Type);
            _captures.Add(@class.Identifier.Text, uxmlTraits);
        }

        uxmlTraits.Properties.Add((property, GetAttributeArgumentValue(attribute)));
    }

    private static bool HasSameType(BasePropertyDeclarationSyntax property, AttributeSyntax attribute)
    {
        var parameter = attribute.ArgumentList!.Arguments.First().Expression;

        if (property.Type is PredefinedTypeSyntax predefinedType)
        {
            if (predefinedType.IsBoolType() &&
                (parameter.IsKind(SyntaxKind.TrueLiteralExpression) ||
                 parameter.IsKind(SyntaxKind.FalseLiteralExpression)))
            {
                return true;
            }

            if (predefinedType.IsStringType() &&
                parameter.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return true;
            }

            if (predefinedType.IsNumericType() &&
                parameter.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                return true;
            }
        }

        if (property.Type is IdentifierNameSyntax identifierName)
        {
            if (identifierName.Identifier.IsKind(SyntaxKind.IdentifierToken) &&
                (parameter.IsKind(SyntaxKind.InvocationExpression) ||
                 parameter.IsKind(SyntaxKind.SimpleMemberAccessExpression)))
            {
                return true;
            }
        }

        return false;
    }

    private static string? GetAttributeArgumentValue(AttributeSyntax attribute)
    {
        return attribute.ArgumentList?.Arguments.Single().Expression switch
        {
            LiteralExpressionSyntax literal => literal.Token.IsKind(SyntaxKind.StringLiteralToken)
                ? literal.Token.ValueText
                : literal.Token.Text,
            InvocationExpressionSyntax invocation => invocation.ArgumentList.Arguments.Single().Expression.GetText().ToString(),
            MemberAccessExpressionSyntax member => member.Parent?.ToString(),
            _ => null
        };
    }
}