using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator.Captures;

internal sealed class UxmlTraitsCapture : BaseCapture
{
    public UxmlTraitsCapture(ClassDeclarationSyntax @class, TypeSyntax baseClassType) : base(@class)
    {
        BaseClassType = baseClassType;
        Properties = new List<(PropertyDeclarationSyntax, AttributeSyntax)>();
    }

    public override string ClassTag => "UxmlTraits";

    public TypeSyntax BaseClassType { get; }
    public List<(PropertyDeclarationSyntax Property, AttributeSyntax Attribute)> Properties { get; }

    public string GetBaseClassName(out IEnumerable<TypeSyntax>? genericTypeArguments)
    {
        if (BaseClassType is GenericNameSyntax genericNameSyntax)
        {
            genericTypeArguments = genericNameSyntax.TypeArgumentList.Arguments;
            return genericNameSyntax.Identifier.Text;
        }

        genericTypeArguments = default;

        if (BaseClassType is IdentifierNameSyntax identifierNameSyntax)
        {
            return identifierNameSyntax.Identifier.Text;
        }

        return BaseClassType.GetText().ToString().Trim();
    }
}