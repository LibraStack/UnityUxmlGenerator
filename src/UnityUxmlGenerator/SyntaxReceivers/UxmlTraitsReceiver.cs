using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityUxmlGenerator.Captures;
using UnityUxmlGenerator.Extensions;

namespace UnityUxmlGenerator.SyntaxReceivers;

internal sealed class UxmlTraitsReceiver : BaseReceiver
{
    private const string AttributeName = "UxmlAttribute";

    private readonly Dictionary<string, UxmlTraitsCapture> _captures = new();

    public IReadOnlyDictionary<string, UxmlTraitsCapture> Captures => _captures;

    public override void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode.IsAttributeWithName(AttributeName, out var attribute) == false)
        {
            return;
        }

        var member = attribute!.GetParent<MemberDeclarationSyntax>();
        if (member is not PropertyDeclarationSyntax property)
        {
            return;
        }

        var @class = property.GetParent<ClassDeclarationSyntax>();
        if (@class.InheritsFromAnyType() == false)
        {
            if (@class is null)
            {
                RegisterDiagnostic(ClassHasNoBaseClassError, property.GetLocation());
            }
            else
            {
                RegisterDiagnostic(ClassHasNoBaseClassError, @class.GetLocation(), @class.Identifier.Text);
            }

            return;
        }

        if (_captures.TryGetValue(@class!.Identifier.Text, out var uxmlTraits) == false)
        {
            uxmlTraits = new UxmlTraitsCapture(@class, @class.BaseList!.Types.First().Type);
            _captures.Add(uxmlTraits.ClassName, uxmlTraits);
        }

        uxmlTraits.Properties.Add((property, attribute));
    }
}