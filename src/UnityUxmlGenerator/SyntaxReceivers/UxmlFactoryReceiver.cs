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
        if (syntaxNode.IsMemberHasAttribute<ClassDeclarationSyntax>(AttributeName,
                out (ClassDeclarationSyntax Class, AttributeSyntax Attribute) result) == false)
        {
            return;
        }

        if (result.Class.InheritsFromAnyType())
        {
            _captures.Add(new UxmlFactoryCapture(result));
        }
        else if (result.Class is not null)
        {
            RegisterDiagnostic(ClassHasNoBaseClassError, result.Class.GetLocation(), result.Class.Identifier.Text);
        }
    }
}