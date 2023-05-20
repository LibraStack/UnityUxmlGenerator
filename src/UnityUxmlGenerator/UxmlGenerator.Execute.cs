﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator;

internal sealed partial class UxmlGenerator
{
    private static CompilationUnitSyntax GetCompilationUnit(TypeDeclarationSyntax typeDeclarationSyntax,
        string? @namespace = null, params MemberDeclarationSyntax[] memberDeclarations)
    {
        if (memberDeclarations.Length != 0)
        {
            typeDeclarationSyntax = typeDeclarationSyntax.AddMembers(ProcessMemberDeclarations(memberDeclarations));
        }

        if (string.IsNullOrEmpty(@namespace))
        {
            // If there is no namespace, attach the pragma directly to the declared type,
            // and skip the namespace declaration. This will produce code as follows:
            //
            // <SYNTAX_TRIVIA>
            // <TYPE_HIERARCHY>
            return
                CompilationUnit()
                    .AddMembers(typeDeclarationSyntax)
                    .NormalizeWhitespace();
        }

        // Prepare the leading trivia for the generated compilation unit.
        // This will produce code as follows:
        //
        // <auto-generated/>
        // #pragma warning disable
        // #nullable enable
        var syntaxTriviaList = TriviaList(
            Comment("// <auto-generated/>"),
            Trivia(PragmaWarningDirectiveTrivia(Token(SyntaxKind.DisableKeyword), true)),
            Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true)));

        // Create the compilation unit with disabled warnings, target namespace and generated type.
        // This will produce code as follows:
        //
        // <SYNTAX_TRIVIA>
        // namespace <NAMESPACE>
        // {
        //     <TYPE_HIERARCHY>
        // }
        return
            CompilationUnit()
                .AddMembers(NamespaceDeclaration(IdentifierName(@namespace!))
                    .WithLeadingTrivia(syntaxTriviaList)
                    .AddMembers(typeDeclarationSyntax))
                .NormalizeWhitespace();
    }

    private static MemberDeclarationSyntax[] ProcessMemberDeclarations(
        IReadOnlyList<MemberDeclarationSyntax> memberDeclarations)
    {
        var annotatedMemberDeclarations = new MemberDeclarationSyntax[memberDeclarations.Count];

        for (var i = 0; i < memberDeclarations.Count; i++)
        {
            annotatedMemberDeclarations[i] = ProcessMemberDeclaration(memberDeclarations[i]);
        }

        return annotatedMemberDeclarations;
    }

    private static MemberDeclarationSyntax ProcessMemberDeclaration(MemberDeclarationSyntax member)
    {
        // [GeneratedCode] is always present.
        member = member
            .WithoutLeadingTrivia()
            .AddAttributeLists(AttributeList(SingletonSeparatedList(
                Attribute(IdentifierName("global::System.CodeDom.Compiler.GeneratedCode"))
                    .AddArgumentListArguments(
                        AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(AssemblyName.Name))),
                        AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(AssemblyName.Version.ToString())))))))
            .WithLeadingTrivia(member.GetLeadingTrivia());

        // [ExcludeFromCodeCoverage] is not supported on interfaces and fields.
        if (member.Kind() is not SyntaxKind.InterfaceDeclaration and not SyntaxKind.FieldDeclaration)
        {
            member = member
                .AddAttributeLists(AttributeList(SingletonSeparatedList(
                    Attribute(IdentifierName("global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage")))));
        }

        return member;
    }
}