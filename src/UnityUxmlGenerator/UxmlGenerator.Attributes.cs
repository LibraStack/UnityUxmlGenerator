using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace UnityUxmlGenerator;

internal sealed partial class UxmlGenerator
{
    private const string AttributeBaseType = "global::System.Attribute";

    private const string UxmlElementClassName = "UxmlElementAttribute";
    private const string UxmlAttributeClassName = "UxmlAttributeAttribute";

    private static SourceText GenerateUxmlElementAttribute()
    {
        return GenerateAttributeClass(UxmlElementClassName);
    }

    private static SourceText GenerateUxmlAttributeAttribute()
    {
        return GenerateAttributeClass(UxmlAttributeClassName, GetUxmlAttributeMembers());
    }

    private static SourceText GenerateAttributeClass(string attributeClassIdentifier,
        IEnumerable<MemberDeclarationSyntax>? members = null)
    {
        return CompilationUnitWidget(
                namespaceIdentifier: AssemblyName.Name,
                member: ClassWidget(
                    identifier: attributeClassIdentifier,
                    modifiers: new[] { SyntaxKind.InternalKeyword, SyntaxKind.SealedKeyword },
                    baseType: SimpleBaseType(IdentifierName(AttributeBaseType)),
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