using Microsoft.CodeAnalysis;

namespace UnityUxmlGenerator.Diagnostics;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor ClassHasNoBaseClassError = new(
        id: "UXMLG001",
        title: "Class has no base class",
        messageFormat: "Class '{0}' must be declared as a partial and be inherited from 'VisualElement' or one of its derived classes.",
        category: typeof(UxmlGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ClassDoesNotInheritFromVisualElementError = new(
        id: "UXMLG002",
        title: "Class does not inherit from VisualElement",
        messageFormat: "Class '{0}' must be declared as a partial and be inherited from 'VisualElement' or one of its derived classes.",
        category: typeof(UxmlGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PropertyAndDefaultValueTypesMismatchError = new(
        id: "UXMLG003",
        title: "Types mismatch",
        messageFormat: "UxmlAttribute for '{0}' property was not created. The default property and attribute value must be of the same type.",
        category: typeof(UxmlGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}