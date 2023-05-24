using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator.Captures;

internal sealed class UxmlFactoryCapture : BaseCapture
{
    public UxmlFactoryCapture(ClassDeclarationSyntax @class, AttributeSyntax attribute) : base(@class)
    {
        Attribute = attribute;
    }

    public override string ClassTag => "UxmlFactory";

    public AttributeSyntax Attribute { get; }
}