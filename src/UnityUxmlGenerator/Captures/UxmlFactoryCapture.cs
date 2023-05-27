using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator.Captures;

internal sealed class UxmlFactoryCapture : BaseCapture
{
    public UxmlFactoryCapture((ClassDeclarationSyntax Class, AttributeSyntax Attribute) data) : base(data.Class)
    {
        Attribute = data.Attribute;
    }

    public override string ClassTag => "UxmlFactory";

    public AttributeSyntax Attribute { get; }
}