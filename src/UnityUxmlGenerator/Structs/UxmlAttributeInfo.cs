using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator.Structs;

public ref struct UxmlAttributeInfo
{
    public string TypeIdentifier { get; set; }
    public string PrivateFieldName { get; set; }
    public string AttributeUxmlName { get; set; }
    public ExpressionSyntax DefaultValueAssignmentExpression { get; set; }
}