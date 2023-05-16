using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityUxmlGenerator.Extensions;

namespace UnityUxmlGenerator.Captures;

internal sealed class UxmlFactoryCapture
{
    public UxmlFactoryCapture(ClassDeclarationSyntax @class)
    {
        Class = @class;
        ClassName = @class.Identifier.Text;
        ClassNamespace = @class.GetParent<NamespaceDeclarationSyntax>()!.Name.ToString();
    }

    public string ClassName { get; }
    public string ClassNamespace { get; }

    public ClassDeclarationSyntax Class { get; }
}