using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        if (attribute!.ArgumentList is not null && attribute.ArgumentList.Arguments.Any())
        {
            if (HasSameType(property, attribute.ArgumentList.Arguments.First()) == false)
            {
                RegisterDiagnostic(PropertyAndDefaultValueTypesMismatchError, property.GetLocation(),
                    property.GetName());
                return;
            }
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

    private static bool HasSameType(BasePropertyDeclarationSyntax property, AttributeArgumentSyntax attributeArgument)
    {
        var parameter = attributeArgument.Expression;

        if (parameter.IsKind(SyntaxKind.DefaultLiteralExpression))
        {
            return true;
        }

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

        SyntaxToken? propertyTypeIdentifier = property.Type switch
        {
            IdentifierNameSyntax identifierName => identifierName.Identifier,
            QualifiedNameSyntax qualifiedNameSyntax => qualifiedNameSyntax.Right.Identifier,
            _ => null
        };

        if (propertyTypeIdentifier is null)
        {
            return false;
        }

        return propertyTypeIdentifier.Value.IsKind(SyntaxKind.IdentifierToken) &&
               (parameter.IsKind(SyntaxKind.InvocationExpression) ||
                parameter.IsKind(SyntaxKind.SimpleMemberAccessExpression));
    }
}