using Microsoft.CodeAnalysis;
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

        var @class = property?.GetParent<ClassDeclarationSyntax>();

        if (@class?.BaseList is null || @class.BaseList.Types.Count == 0)
        {
            return;
        }

        if (_captures.TryGetValue(@class.Identifier.Text, out var uxmlTraits) == false)
        {
            uxmlTraits = new UxmlTraitsCapture(@class, @class.BaseList.Types.First().Type);
            _captures.Add(@class.Identifier.Text, uxmlTraits);
        }

        uxmlTraits.Properties.Add((property!, GetAttributeArgumentValue(attribute)));
    }

    private static string? GetAttributeArgumentValue(AttributeSyntax attribute)
    {
        return attribute.ArgumentList?.Arguments.Single().Expression switch
        {
            LiteralExpressionSyntax literal => literal.Token.ValueText,
            InvocationExpressionSyntax invocation => invocation.ArgumentList.Arguments.Single().Expression.GetText().ToString(),
            _ => null
        };
    }
}