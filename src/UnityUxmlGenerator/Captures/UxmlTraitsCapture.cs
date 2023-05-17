using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityUxmlGenerator.Extensions;

namespace UnityUxmlGenerator.Captures;

internal sealed class UxmlTraitsCapture
{
    public UxmlTraitsCapture(ClassDeclarationSyntax @class, TypeSyntax baseClassType)
    {
        Class = @class;
        ClassName = @class.Identifier.Text;
        ClassNamespace = @class.GetParent<NamespaceDeclarationSyntax>()!.Name.ToString();

        BaseClassType = baseClassType;
        Properties = new List<(PropertyDeclarationSyntax property, string? DefaultValue)>();
    }

    public string ClassName { get; }
    public string ClassNamespace { get; }

    public TypeSyntax BaseClassType { get; }
    public ClassDeclarationSyntax Class { get; }

    public List<(PropertyDeclarationSyntax property, string? DefaultValue)> Properties { get; }

    public string GetBaseClassName(out TypeSyntax? genericTypeSyntax)
    {
        if (BaseClassType is GenericNameSyntax genericNameSyntax)
        {
            genericTypeSyntax = genericNameSyntax.TypeArgumentList.Arguments[0];
            return genericNameSyntax.Identifier.Text;
        }

        genericTypeSyntax = default;

        if (BaseClassType is IdentifierNameSyntax identifierNameSyntax)
        {
            return identifierNameSyntax.Identifier.Text;
        }

        return BaseClassType.GetText().ToString().Trim();
    }
}