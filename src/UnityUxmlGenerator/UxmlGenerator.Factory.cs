using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using UnityUxmlGenerator.Captures;

namespace UnityUxmlGenerator;

internal sealed partial class UxmlGenerator
{
    private const string FactoryBaseTypeIdentifier = "global::UnityEngine.UIElements.UxmlFactory<{0}, UxmlTraits>";

    private static SourceText GenerateUxmlFactory(UxmlFactoryCapture capture)
    {
        return CompilationUnitWidget(
                members: NamespaceWidget(
                    identifier: capture.ClassNamespace,
                    member: ClassWidget(
                        identifier: capture.ClassName,
                        modifier: SyntaxKind.PartialKeyword,
                        member: ClassWidget(
                            identifier: "UxmlFactory",
                            modifiers: new[] { SyntaxKind.PublicKeyword, SyntaxKind.NewKeyword },
                            baseType: SimpleBaseType(IdentifierName(string.Format(FactoryBaseTypeIdentifier, capture.ClassName))),
                            addGeneratedCodeAttributes: true
                        ))),
                normalizeWhitespace: true)
            .GetText(Encoding.UTF8);
    }
}