using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityUxmlGenerator.Captures;
using UnityUxmlGenerator.Extensions;

namespace UnityUxmlGenerator.SyntaxReceivers;

internal sealed class UxmlFactoryReceiver : ISyntaxReceiver
{
    private const string AttributeName = "UxmlElement";

    private readonly List<UxmlFactoryCapture> _captures = new();

    public IEnumerable<UxmlFactoryCapture> Captures => _captures;

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not AttributeSyntax
            {
                Name: IdentifierNameSyntax { Identifier.Text: AttributeName }
            } attribute)
        {
            return;
        }

        var @class = attribute.GetParent<ClassDeclarationSyntax>();

        if (@class is null)
        {
            return;
        }

        _captures.Add(new UxmlFactoryCapture(@class));
    }
}