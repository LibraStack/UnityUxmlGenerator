using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using UnityUxmlGenerator.Captures;
using UnityUxmlGenerator.Extensions;

namespace UnityUxmlGenerator;

internal sealed partial class UxmlGenerator
{
    private static SourceText GenerateUxmlTraits(GeneratorExecutionContext context, UxmlTraitsCapture capture)
    {
        var @class = ClassDeclaration(capture.ClassName).AddModifiers(Token(SyntaxKind.PartialKeyword));

        var traitsClass = GetTraitsClass(context, capture);

        return GetCompilationUnit(@class, capture.ClassNamespace, traitsClass).GetText(Encoding.UTF8);
    }

    private static MemberDeclarationSyntax GetTraitsClass(GeneratorExecutionContext context, UxmlTraitsCapture capture)
    {
        var uxmlTraitsBaseList = SimpleBaseType(IdentifierName($"{GetBaseClassName(context, capture)}.UxmlTraits"));

        return
            ClassDeclaration("UxmlTraits")
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.NewKeyword)))
                .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(uxmlTraitsBaseList)))
                .WithMembers(List(GetTraitsClassMembers(capture)));
    }

    private static string GetBaseClassName(GeneratorExecutionContext context, UxmlTraitsCapture capture)
    {
        var baseClassName = capture.GetBaseClassName(out var genericClass);
        var baseClassNamespace = capture.BaseClassType.GetTypeNamespace(context);

        if (genericClass is null)
        {
            return $"global::{baseClassNamespace}.{baseClassName}";
        }

        if (genericClass is PredefinedTypeSyntax predefinedTypeSyntax)
        {
            return $"global::{baseClassNamespace}.{baseClassName}<{predefinedTypeSyntax.Keyword.Text}>";
        }

        if (genericClass is IdentifierNameSyntax customTypeSyntax)
        {
            var genericClassName = customTypeSyntax.Identifier.Text;
            var genericClassNamespace = customTypeSyntax.GetTypeNamespace(context);

            return $"global::{baseClassNamespace}.{baseClassName}<global::{genericClassNamespace}.{genericClassName}>";
        }

        return string.Empty;
    }

    private static IEnumerable<MemberDeclarationSyntax> GetTraitsClassMembers(UxmlTraitsCapture capture)
    {
        var members = new List<MemberDeclarationSyntax>(GetAttributeFields(capture));

        var initMethodBody = new List<StatementSyntax>
        {
            ExpressionStatement(
                InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, BaseExpression(),
                        IdentifierName("Init")))
                    .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                    {
                        Argument(IdentifierName("visualElement")),
                        Token(SyntaxKind.CommaToken),
                        Argument(IdentifierName("bag")),
                        Token(SyntaxKind.CommaToken),
                        Argument(IdentifierName("context"))
                    })))),
            LocalDeclarationStatement(
                VariableDeclaration(IdentifierName(Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList())))
                    .WithVariables(SingletonSeparatedList(
                        VariableDeclarator(Identifier("control"))
                            .WithInitializer(EqualsValueClause(CastExpression(IdentifierName(capture.ClassName), IdentifierName("visualElement")))))))
        };

        initMethodBody.AddRange(GetAttributeValueAssignments(capture));

        var initMethod =
            MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("Init"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(new SyntaxNodeOrToken[]
                {
                    Parameter(Identifier("visualElement"))
                        .WithType(IdentifierName("global::UnityEngine.UIElements.VisualElement")),
                    Token(SyntaxKind.CommaToken),
                    Parameter(Identifier("bag"))
                        .WithType(IdentifierName("global::UnityEngine.UIElements.IUxmlAttributes")),
                    Token(SyntaxKind.CommaToken),
                    Parameter(Identifier("context"))
                        .WithType(IdentifierName("global::UnityEngine.UIElements.CreationContext"))
                })))
                .WithBody(Block(initMethodBody));

        members.Add(initMethod);

        return ProcessMemberDeclarations(members);
    }

    private static IEnumerable<MemberDeclarationSyntax> GetAttributeFields(UxmlTraitsCapture capture)
    {
        var fields = new List<MemberDeclarationSyntax>();

        foreach (var (property, uxmlAttributeDefaultValue) in capture.Properties)
        {
            var propertyName = property.GetName();

            var fieldName = propertyName.ToPrivateFieldName();

            var attributeType = "UxmlStringAttributeDescription";
            var attributeUxmlName = propertyName.ToDashCase();
            var attributeDefaultValue = uxmlAttributeDefaultValue ?? string.Empty;

            fields.Add(GetAttributeFieldDeclaration(attributeType, fieldName, attributeUxmlName,
                attributeDefaultValue));
        }

        return fields;
    }

    private static FieldDeclarationSyntax GetAttributeFieldDeclaration(string attributeType, string fieldName,
        string attributeName, string attributeDefaultValue)
    {
        return
            FieldDeclaration(VariableDeclaration(IdentifierName($"global::UnityEngine.UIElements.{attributeType}"))
                    .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(fieldName))
                        .WithInitializer(EqualsValueClause(ImplicitObjectCreationExpression()
                            .WithInitializer(InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                                SeparatedList<ExpressionSyntax>(new SyntaxNodeOrToken[]
                                {
                                    AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName("name"),
                                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(attributeName))),
                                    Token(SyntaxKind.CommaToken),
                                    AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName("defaultValue"),
                                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(attributeDefaultValue)))
                                }))))))))
                .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword)));
    }

    private static IEnumerable<StatementSyntax> GetAttributeValueAssignments(UxmlTraitsCapture capture)
    {
        var attributeValueAssignments = new List<StatementSyntax>();

        foreach (var (property, _) in capture.Properties)
        {
            var propertyName = property.GetName();
            var fieldName = propertyName.ToPrivateFieldName();

            attributeValueAssignments.Add(GetAttributeValueAssignment(propertyName, fieldName));
        }

        return attributeValueAssignments;
    }

    private static StatementSyntax GetAttributeValueAssignment(string propertyName, string fieldName)
    {
        return
            ExpressionStatement(
                AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("control"),
                        IdentifierName(propertyName)),
                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(fieldName), IdentifierName("GetValueFromBag")))
                        .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Argument(IdentifierName("bag")),
                                Token(SyntaxKind.CommaToken),
                                Argument(IdentifierName("context"))
                            })))));
    }
}