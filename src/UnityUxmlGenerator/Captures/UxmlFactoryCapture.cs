using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator.Captures;

internal sealed class UxmlFactoryCapture : BaseCapture
{
    public UxmlFactoryCapture(ClassDeclarationSyntax @class) : base(@class)
    {
    }

    public override string ClassTag => "UxmlFactory";
}