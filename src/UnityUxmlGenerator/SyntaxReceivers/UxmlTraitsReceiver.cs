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

        if (@class is null || IsValid(@class, out var baseClassIdentifier) == false)
        {
            return;
        }

        if (_captures.TryGetValue(@class.Identifier.Text, out var uxmlTraits) == false)
        {
            uxmlTraits = new UxmlTraitsCapture(@class, baseClassIdentifier!);
            _captures.Add(@class.Identifier.Text, uxmlTraits);
        }

        uxmlTraits.Properties.Add((property!.Identifier.Text, GetAttributeArgumentValue(attribute)));
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

    private static bool IsValid(ClassDeclarationSyntax @class, out string? baseClassIdentifier)
    {
        if (@class.BaseList == null)
        {
            baseClassIdentifier = default;
            return false;
        }

        baseClassIdentifier = @class.BaseList.Types.First().Type.GetText().ToString().Trim();
        return string.IsNullOrWhiteSpace(baseClassIdentifier) == false;
    }
}