using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using UnityUxmlGenerator.Captures;
using UnityUxmlGenerator.Diagnostics;
using UnityUxmlGenerator.Extensions;
using UnityUxmlGenerator.SyntaxReceivers;

namespace UnityUxmlGenerator;

[Generator]
internal sealed partial class UxmlGenerator : ISourceGenerator
{
    private const string VisualElementFullName = "UnityEngine.UIElements.VisualElement";

    private static readonly AssemblyName AssemblyName = typeof(UxmlGenerator).Assembly.GetName();

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new VisualElementReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        context.AddSource($"{nameof(UxmlElementClassName)}.g.cs", GenerateUxmlElementAttribute());
        context.AddSource($"{nameof(UxmlAttributeClassName)}.g.cs", GenerateUxmlAttributeAttribute());

        if (context.SyntaxReceiver is not VisualElementReceiver receiver ||
            context.CancellationToken.IsCancellationRequested)
        {
            return;
        }

        foreach (var uxmlElement in receiver.UxmlFactoryReceiver.Captures)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (IsValidClass(context, uxmlElement.Class) &&
                IsValidAttribute(context, uxmlElement.Attribute))
            {
                AddSource(context, uxmlElement, GenerateUxmlFactory(uxmlElement));
            }
        }

        foreach (var capture in receiver.UxmlTraitsReceiver.Captures)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var uxmlTraits = capture.Value;

            if (IsValidClass(context, uxmlTraits.Class))
            {
                AddSource(context, capture.Value, GenerateUxmlTraits(context, capture.Value));
            }
        }

        ReportDiagnostics(context, receiver.UxmlTraitsReceiver.Diagnostics);
        ReportDiagnostics(context, receiver.UxmlFactoryReceiver.Diagnostics);
    }

    private static bool IsValidClass(GeneratorExecutionContext context, BaseTypeDeclarationSyntax @class)
    {
        if (@class.InheritsFromFullyQualifiedName(context, VisualElementFullName))
        {
            return true;
        }

        ReportClassDoesNotInheritFromVisualElementError(context, @class);
        return false;
    }

    private static bool IsValidAttribute(GeneratorExecutionContext context, AttributeSyntax attribute)
    {
        return attribute.GetTypeNamespace(context) == AssemblyName.Name;
    }

    private static void AddSource(GeneratorExecutionContext context, BaseCapture capture, SourceText sourceText)
    {
        context.AddSource($"{capture.ClassName}.{capture.ClassTag}.g.cs", sourceText);
    }

    private static void ReportDiagnostics(GeneratorExecutionContext context, IEnumerable<Diagnostic> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
        {
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void ReportClassDoesNotInheritFromVisualElementError(GeneratorExecutionContext context,
        BaseTypeDeclarationSyntax @class)
    {
        context.ReportDiagnostic(
            ClassDoesNotInheritFromVisualElementError.CreateDiagnostic(@class.GetLocation(), @class.Identifier.Text));
    }
}