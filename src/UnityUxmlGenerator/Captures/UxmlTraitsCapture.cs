using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator.Captures;

internal sealed class UxmlTraitsCapture
{
    public UxmlTraitsCapture(ClassDeclarationSyntax @class, string baseClassIdentifier)
    {
        Class = @class;
        ClassIdentifier = @class.Identifier.Text;
        BaseClassIdentifier = baseClassIdentifier;
        Properties = new List<(string PropertyName, string? DefaultValue)>();
    }

    public string ClassIdentifier { get; }
    public string BaseClassIdentifier { get; }

    public ClassDeclarationSyntax Class { get; }
    public List<(string PropertyName, string? DefaultValue)> Properties { get; }
}