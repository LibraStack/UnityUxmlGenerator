using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator.Captures;

internal sealed class UxmlFactoryCapture
{
    public UxmlFactoryCapture(ClassDeclarationSyntax @class)
    {
        Class = @class;
        ClassIdentifier = @class.Identifier.Text;
    }

    public string ClassIdentifier { get; }
    public ClassDeclarationSyntax Class { get; }
}