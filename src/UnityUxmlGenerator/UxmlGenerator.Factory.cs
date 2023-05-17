using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using UnityUxmlGenerator.Captures;

namespace UnityUxmlGenerator;

internal sealed partial class UxmlGenerator
{
    private static SourceText GenerateUxmlFactory(UxmlFactoryCapture capture)
    {
        var @class = ClassDeclaration(capture.ClassName).AddModifiers(Token(SyntaxKind.PartialKeyword));

        var factoryClass = GetFactoryClass(capture);

        return GetCompilationUnit(@class, capture.ClassNamespace, factoryClass).GetText(Encoding.UTF8);
    }

    private static MemberDeclarationSyntax GetFactoryClass(UxmlFactoryCapture capture)
    {
        var uxmlFactoryBaseList =
            SimpleBaseType(
                IdentifierName($"global::UnityEngine.UIElements.UxmlFactory<{capture.ClassName}, UxmlTraits>"));

        return
            ClassDeclaration("UxmlFactory")
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.NewKeyword)))
                .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(uxmlFactoryBaseList)));
    }
}