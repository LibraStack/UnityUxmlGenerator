﻿using Microsoft.CodeAnalysis;

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

    public static readonly DiagnosticDescriptor PropertyTypeIsNotSupportedError = new(
        id: "UXMLG004",
        title: "Property type is not supported",
        messageFormat: "Property type '{0}' can not be used as an attribute.",
        category: typeof(UxmlGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor IncorrectEnumDefaultValueTypeError = new(
        id: "UXMLG005",
        title: "Type cannot be the default value for an enum",
        messageFormat: "Type '{0}' cannot be the default value for an enum.",
        category: typeof(UxmlGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}