using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityUxmlGenerator.Captures;
using UnityUxmlGenerator.Extensions;

namespace UnityUxmlGenerator.SyntaxReceivers;

internal sealed class UxmlFactoryReceiver : BaseReceiver
{
    private const string AttributeName = "UxmlElement";

    private readonly List<UxmlFactoryCapture> _captures = new();

    public IEnumerable<UxmlFactoryCapture> Captures => _captures;

    public override void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not AttributeSyntax
            {
                Name: IdentifierNameSyntax { Identifier.Text: AttributeName }
            } attribute)
        {
            return;
        }

        var @class = attribute.GetParent<ClassDeclarationSyntax>();

        if (@class.InheritsFromAnyType())
        {
            _captures.Add(new UxmlFactoryCapture(@class!));
        }
        else if (@class is not null)
        {
            RegisterDiagnostic(ClassHasNoBaseClassError, @class.GetLocation(), @class.Identifier.Text);
        }
    }
}