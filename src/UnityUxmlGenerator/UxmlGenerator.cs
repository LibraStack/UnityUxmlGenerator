using Microsoft.CodeAnalysis;
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
        context.AddSource($"{nameof(UxmlElementAttribute)}.g.cs", UxmlElementAttribute);
        context.AddSource($"{nameof(UxmlAttributeAttribute)}.g.cs", UxmlAttributeAttribute);

        if (context.SyntaxReceiver is not VisualElementReceiver receiver)
        {
            return;
        }

        foreach (var uxmlElement in receiver.UxmlFactoryReceiver.Captures)
        {
            if (uxmlElement.Class.InheritsFromFullyQualifiedName(context, VisualElementFullName))
            {
                context.AddSource($"{uxmlElement.ClassName}.UxmlFactory.g.cs", GenerateUxmlFactory(uxmlElement));
            }
        }

        foreach (var capture in receiver.UxmlTraitsReceiver.Captures)
        {
            var traitsCapture = capture.Value;

            if (traitsCapture.Class.InheritsFromFullyQualifiedName(context, VisualElementFullName))
            {
                context.AddSource($"{traitsCapture.ClassName}.UxmlTraits.g.cs", GenerateUxmlTraits(context, traitsCapture));
            }
        }
    }
}