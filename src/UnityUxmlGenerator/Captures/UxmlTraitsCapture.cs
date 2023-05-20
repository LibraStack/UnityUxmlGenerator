using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator.Captures;

internal sealed class UxmlTraitsCapture : BaseCapture
{
    public UxmlTraitsCapture(ClassDeclarationSyntax @class, TypeSyntax baseClassType) : base(@class)
    {
        BaseClassType = baseClassType;
        Properties = new List<(PropertyDeclarationSyntax property, string? DefaultValue)>();
    }

    public override string ClassTag => "UxmlTraits";

    public TypeSyntax BaseClassType { get; }
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