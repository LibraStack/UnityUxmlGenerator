using Microsoft.CodeAnalysis;
using UnityUxmlGenerator.SyntaxReceivers;

namespace UnityUxmlGenerator;

[Generator]
internal sealed partial class UxmlGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new VisualElementReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        context.AddSource("UxmlElementAttribute.g.cs", UxmlElementAttribute);
        context.AddSource("UxmlAttributeAttribute.g.cs", UxmlAttributeAttribute);

        if (context.SyntaxReceiver is not VisualElementReceiver receiver)
        {
            return;
        }

        foreach (var uxmlElement in receiver.UxmlFactoryReceiver.Captures)
        {
            context.AddSource($"{uxmlElement.ClassIdentifier}.UxmlFactory.g.cs", GenerateUxmlFactory(uxmlElement));
        }

        foreach (var capture in receiver.UxmlTraitsReceiver.Captures)
        {
            context.AddSource($"{capture.Key}.UxmlTraits.g.cs", GenerateUxmlTraits(capture.Value));
        }
    }
}