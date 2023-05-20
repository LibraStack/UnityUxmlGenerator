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

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new VisualElementReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        context.AddSource($"{nameof(UxmlElementClassName)}.g.cs", GenerateUxmlElementAttribute());
        context.AddSource($"{nameof(UxmlAttributeClassName)}.g.cs", GenerateUxmlAttributeAttribute());

        if (context.SyntaxReceiver is not VisualElementReceiver receiver)
        {
            return;
        }

        foreach (var uxmlElement in receiver.UxmlFactoryReceiver.Captures)
        {
            AddSource(context, uxmlElement, GenerateUxmlFactory(uxmlElement));
        }

        foreach (var capture in receiver.UxmlTraitsReceiver.Captures)
        {
            AddSource(context, capture.Value, GenerateUxmlTraits(context, capture.Value));
        }

        ReportDiagnostics(context, receiver.UxmlTraitsReceiver.Diagnostics);
        ReportDiagnostics(context, receiver.UxmlFactoryReceiver.Diagnostics);
    }

    private static void AddSource(GeneratorExecutionContext context, BaseCapture capture, SourceText sourceText)
    {
        if (capture.Class.InheritsFromFullyQualifiedName(context, VisualElementFullName))
        {
            context.AddSource($"{capture.ClassName}.{capture.ClassTag}.g.cs", sourceText);
        }
        else
        {
            ReportClassDoesNotInheritFromVisualElementError(context, capture.Class);
        }
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