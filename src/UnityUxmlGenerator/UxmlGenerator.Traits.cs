using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using UnityUxmlGenerator.Captures;
using UnityUxmlGenerator.Extensions;
using UnityUxmlGenerator.Structs;

namespace UnityUxmlGenerator;

internal sealed partial class UxmlGenerator
{
    private const string UnityColorTypeFullName = "global::UnityEngine.Color";
    private const string UnityUiElementsFullName = "global::UnityEngine.UIElements.{0}";
    private const string UxmlColorAttributeDescription = "UxmlColorAttributeDescription";

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
                .WithMembers(List(GetTraitsClassMembers(context, capture)));
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

    private static IEnumerable<MemberDeclarationSyntax> GetTraitsClassMembers(GeneratorExecutionContext context, 
        UxmlTraitsCapture capture)
    {
        var members = new List<MemberDeclarationSyntax>(GetAttributeFields(context, capture));

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
                        .WithType(IdentifierName(string.Format(UnityUiElementsFullName, "VisualElement"))),
                    Token(SyntaxKind.CommaToken),
                    Parameter(Identifier("bag"))
                        .WithType(IdentifierName(string.Format(UnityUiElementsFullName, "IUxmlAttributes"))),
                    Token(SyntaxKind.CommaToken),
                    Parameter(Identifier("context"))
                        .WithType(IdentifierName(string.Format(UnityUiElementsFullName, "CreationContext")))
                })))
                .WithBody(Block(initMethodBody));

        members.Add(initMethod);

        return ProcessMemberDeclarations(members);
    }

    private static IEnumerable<MemberDeclarationSyntax> GetAttributeFields(GeneratorExecutionContext context,
        UxmlTraitsCapture capture)
    {
        var fields = new List<MemberDeclarationSyntax>();

        foreach (var (property, uxmlAttributeDefaultValue) in capture.Properties)
        {
            fields.Add(GetAttributeFieldDeclaration(GetAttributeInfo(context, property, uxmlAttributeDefaultValue)));
        }

        return fields;
    }

    private static UxmlAttributeInfo GetAttributeInfo(GeneratorExecutionContext context,
        PropertyDeclarationSyntax property, string? uxmlAttributeDefaultValue)
    {
        var propertyName = property.GetName();
        
        var info = new UxmlAttributeInfo
        {
            TypeIdentifier = GetPropertyTypeIdentifier(context, property, out var predefinedTypeSyntax),
            PrivateFieldName = propertyName.ToPrivateFieldName(),
            AttributeUxmlName = propertyName.ToDashCase()
        };

        if (uxmlAttributeDefaultValue is null || predefinedTypeSyntax is null)
        {
            info.DefaultValueAssignmentExpression =
                LiteralExpression(SyntaxKind.DefaultLiteralExpression, Token(SyntaxKind.DefaultKeyword));
            return info;
        }

        if (predefinedTypeSyntax.IsBoolType())
        {
            info.DefaultValueAssignmentExpression = IdentifierName(uxmlAttributeDefaultValue);
            return info;
        }

        if (predefinedTypeSyntax.IsStringType())
        {
            info.DefaultValueAssignmentExpression = LiteralExpression(SyntaxKind.StringLiteralExpression,
                Literal(uxmlAttributeDefaultValue));
            return info;
        }

        if (predefinedTypeSyntax.IsNumericType())
        {
            info.DefaultValueAssignmentExpression = LiteralExpression(SyntaxKind.NumericLiteralExpression,
                Literal(uxmlAttributeDefaultValue, uxmlAttributeDefaultValue));
            return info;
        }

        if (info.TypeIdentifier == UxmlColorAttributeDescription)
        {
            info.DefaultValueAssignmentExpression = IdentifierName($"global::UnityEngine.{uxmlAttributeDefaultValue}");
            return info;
        }

        info.DefaultValueAssignmentExpression = IdentifierName(uxmlAttributeDefaultValue);
        return info;
    }

    private static string GetPropertyTypeIdentifier(GeneratorExecutionContext context,
        BasePropertyDeclarationSyntax property, out PredefinedTypeSyntax? predefinedTypeSyntax)
    {
        switch (property.Type)
        {
            case PredefinedTypeSyntax predefinedType:
            {
                var propertyTypeIdentifier = predefinedType.Keyword.Text.FirstCharToUpper();

                predefinedTypeSyntax = predefinedType;

                return $"Uxml{propertyTypeIdentifier}AttributeDescription";
            }

            case IdentifierNameSyntax customTypeSyntax:
            {
                var type = customTypeSyntax.Identifier.Text;
                var typeNamespace = customTypeSyntax.GetTypeNamespace(context);
                var propertyTypeText = $"global::{typeNamespace}.{type}";

                predefinedTypeSyntax = default;

                return propertyTypeText == UnityColorTypeFullName
                    ? UxmlColorAttributeDescription
                    : $"UxmlEnumAttributeDescription<{propertyTypeText}>";
            }

            default:
                predefinedTypeSyntax = default;
                return property.Type.GetText().ToString().Trim();
        }
    }

    private static FieldDeclarationSyntax GetAttributeFieldDeclaration(UxmlAttributeInfo attributeInfo)
    {
        return
            FieldDeclaration(VariableDeclaration(
                        IdentifierName(string.Format(UnityUiElementsFullName, attributeInfo.TypeIdentifier)))
                    .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(attributeInfo.PrivateFieldName))
                        .WithInitializer(EqualsValueClause(ImplicitObjectCreationExpression()
                            .WithInitializer(InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                                SeparatedList<ExpressionSyntax>(new SyntaxNodeOrToken[]
                                {
                                    AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName("name"), 
                                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(attributeInfo.AttributeUxmlName))),
                                    Token(SyntaxKind.CommaToken),
                                    AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName("defaultValue"), 
                                        attributeInfo.DefaultValueAssignmentExpression)
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