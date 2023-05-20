using Microsoft.CodeAnalysis;
using UnityUxmlGenerator.Diagnostics;

namespace UnityUxmlGenerator.SyntaxReceivers;

internal abstract class BaseReceiver : ISyntaxReceiver
{
    private readonly List<Diagnostic> _diagnostics = new();

    public IEnumerable<Diagnostic> Diagnostics => _diagnostics;

    public abstract void OnVisitSyntaxNode(SyntaxNode syntaxNode);

    protected void RegisterDiagnostic(DiagnosticDescriptor diagnosticDescriptor, Location location,
        params object[] args)
    {
        _diagnostics.Add(diagnosticDescriptor.CreateDiagnostic(location, args));
    }
}