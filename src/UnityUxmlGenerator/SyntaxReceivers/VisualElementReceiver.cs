using Microsoft.CodeAnalysis;

namespace UnityUxmlGenerator.SyntaxReceivers;

internal sealed class VisualElementReceiver : ISyntaxReceiver
{
    public UxmlFactoryReceiver UxmlFactoryReceiver { get; } = new();
    public UxmlTraitsReceiver UxmlTraitsReceiver { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        UxmlFactoryReceiver.OnVisitSyntaxNode(syntaxNode);
        UxmlTraitsReceiver.OnVisitSyntaxNode(syntaxNode);
    }
}