using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityUxmlGenerator.Extensions;

namespace UnityUxmlGenerator.Captures;

internal abstract class BaseCapture
{
    protected BaseCapture(ClassDeclarationSyntax @class)
    {
        Class = @class;
        ClassName = @class.Identifier.Text;
        ClassNamespace = @class.GetParent<NamespaceDeclarationSyntax>()?.Name.ToString();
    }

    public string ClassName { get; }
    public string? ClassNamespace { get; }
    public abstract string ClassTag { get; }

    public ClassDeclarationSyntax Class { get; }
}