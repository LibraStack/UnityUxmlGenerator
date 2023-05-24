using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator.Structs;

public ref struct UxmlAttributeInfo
{
    public string PropertyName { get; init; }
    public string PrivateFieldName { get; init; }
    public string AttributeUxmlName { get; init; }

    public string TypeIdentifier { get; set; }
    public ExpressionSyntax DefaultValueAssignmentExpression { get; set; }
}