using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace UnityUxmlGenerator;

internal sealed partial class UxmlGenerator
{
    private const string AttributeBaseType = "global::System.Attribute";

    private const string AttributeClassTarget = "Class";
    private const string AttributePropertyTarget = "Property";

    private const string UxmlElementClassName = "UxmlElementAttribute";
    private const string UxmlAttributeClassName = "UxmlAttributeAttribute";

    private static SourceText GenerateUxmlElementAttribute()
    {
        return GenerateAttributeClass(UxmlElementClassName, AttributeClassTarget);
    }

    private static SourceText GenerateUxmlAttributeAttribute()
    {
        return GenerateAttributeClass(UxmlAttributeClassName, AttributePropertyTarget, GetUxmlAttributeMembers());
    }

    private static SourceText GenerateAttributeClass(string attributeClassIdentifier, string attributeTarget,
        IEnumerable<MemberDeclarationSyntax>? members = null)
    {
        return CompilationUnitWidget(
                namespaceIdentifier: AssemblyName.Name,
                member: ClassWidget(
                    identifier: attributeClassIdentifier,
                    modifiers: new[] { SyntaxKind.InternalKeyword, SyntaxKind.SealedKeyword },
                    baseType: SimpleBaseType(IdentifierName(AttributeBaseType)),
                    attribute: AttributeWidget(
                        identifier: "global::System.AttributeUsage",
                        arguments: new[]
                        {
                            AttributeArgument(MemberAccessWidget(
                                identifier: "global::System.AttributeTargets",
                                memberName: attributeTarget)),
                            AttributeArgument(AssignmentWidget(
                                left: IdentifierName("AllowMultiple"),
                                right: LiteralExpression(SyntaxKind.FalseLiteralExpression))),
                            AttributeArgument(AssignmentWidget(
                                left: IdentifierName("Inherited"),
                                right: LiteralExpression(SyntaxKind.FalseLiteralExpression)))
                        }),
                    members: members,
                    addGeneratedCodeAttributes: true),
                normalizeWhitespace: true)
            .GetText(Encoding.UTF8);
    }

    private static IEnumerable<MemberDeclarationSyntax> GetUxmlAttributeMembers()
    {
        return new MemberDeclarationSyntax[]
        {
            ConstructorWidget(
                identifier: UxmlAttributeClassName,
                modifier: SyntaxKind.PublicKeyword,
                parameter: ParameterWidget(
                    identifier: "defaultValue",
                    type: NullableType(PredefinedType(Token(SyntaxKind.ObjectKeyword))),
                    addDefaultKeyword: true),
                bodyStatement: AssignmentStatementWidget(
                    left: IdentifierName("DefaultValue"),
                    right: IdentifierName("defaultValue")),
                addGeneratedCodeAttributes: true
            ),
            PropertyWidget(
                identifier: "DefaultValue",
                type: NullableType(PredefinedType(Token(SyntaxKind.ObjectKeyword))),
                modifier: SyntaxKind.PublicKeyword,
                accessor: SyntaxKind.GetAccessorDeclaration,
                addGeneratedCodeAttributes: true
            )
        };
    }
}