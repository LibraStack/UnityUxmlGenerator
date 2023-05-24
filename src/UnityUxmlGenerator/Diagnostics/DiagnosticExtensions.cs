using Microsoft.CodeAnalysis;

namespace UnityUxmlGenerator.Diagnostics;

internal static class DiagnosticExtensions
{
    public static Diagnostic CreateDiagnostic(this DiagnosticDescriptor descriptor, Location location, params object?[]? args)
    {
        return Diagnostic.Create(descriptor, location, args);
    }
}