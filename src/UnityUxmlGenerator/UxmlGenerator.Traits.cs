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
        return CompilationUnitWidget(
                namespaceIdentifier: capture.ClassNamespace,
                members: ClassWidget(
                    identifier: capture.ClassName,
                    modifier: SyntaxKind.PartialKeyword,
                    member: ClassWidget(
                        identifier: "UxmlTraits",
                        modifiers: new[] { SyntaxKind.PublicKeyword, SyntaxKind.NewKeyword },
                        baseType: SimpleBaseType(IdentifierName($"{GetBaseClassName(context, capture)}.UxmlTraits")),
                        members: GetTraitsClassMembers(context, capture),
                        addGeneratedCodeAttributes: true
                    )),
                normalizeWhitespace: true)
            .GetText(Encoding.UTF8);
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

    private static MemberDeclarationSyntax[] GetTraitsClassMembers(GeneratorExecutionContext context,
        UxmlTraitsCapture capture)
    {
        var members = new List<MemberDeclarationSyntax>(GetAttributeFields(context, capture));

        var initMethodBody = new List<StatementSyntax>
        {
            MethodBaseCallWidget(
                identifier: "Init",
                arguments: new[]
                {
                    Argument(IdentifierName("visualElement")),
                    Argument(IdentifierName("bag")),
                    Argument(IdentifierName("context"))
                }),
            LocalVariableWidget(
                identifier: "control",
                initializer: CastExpressionWidget(
                    identifier: "visualElement",
                    typeToCast: IdentifierName(capture.ClassName)))
        };

        initMethodBody.AddRange(GetAttributeValueAssignments(context, capture));

        var initMethod = MethodWidget(
            identifier: "Init",
            type: PredefinedType(Token(SyntaxKind.VoidKeyword)),
            modifiers: new[] { SyntaxKind.PublicKeyword, SyntaxKind.OverrideKeyword },
            parameters: new[]
            {
                ParameterWidget(
                    identifier: "visualElement",
                    type: IdentifierName(string.Format(UnityUiElementsFullName, "VisualElement"))),
                ParameterWidget(
                    identifier: "bag",
                    type: IdentifierName(string.Format(UnityUiElementsFullName, "IUxmlAttributes"))),
                ParameterWidget(
                    identifier: "context",
                    type: IdentifierName(string.Format(UnityUiElementsFullName, "CreationContext"))),
            },
            bodyStatements: initMethodBody.ToArray(),
            addGeneratedCodeAttributes: true
        );

        members.Add(initMethod);

        return members.ToArray();
    }

    private static IEnumerable<MemberDeclarationSyntax> GetAttributeFields(GeneratorExecutionContext context,
        UxmlTraitsCapture capture)
    {
        var fields = new List<MemberDeclarationSyntax>();

        foreach (var (property, uxmlAttributeDefaultValue) in capture.Properties)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

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
            TypeIdentifier = GetPropertyTypeIdentifier(context, property, out var typeSyntax, out var typeNamespace),
            PrivateFieldName = propertyName.ToPrivateFieldName(),
            AttributeUxmlName = propertyName.ToDashCase()
        };

        if (uxmlAttributeDefaultValue is null || typeSyntax is null)
        {
            info.DefaultValueAssignmentExpression =
                LiteralExpression(SyntaxKind.DefaultLiteralExpression, Token(SyntaxKind.DefaultKeyword));
            return info;
        }

        if (typeSyntax.IsBoolType())
        {
            info.DefaultValueAssignmentExpression = IdentifierName(uxmlAttributeDefaultValue);
            return info;
        }

        if (typeSyntax.IsStringType())
        {
            info.DefaultValueAssignmentExpression = LiteralExpression(SyntaxKind.StringLiteralExpression,
                Literal(uxmlAttributeDefaultValue));
            return info;
        }

        if (typeSyntax.IsNumericType())
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

        info.DefaultValueAssignmentExpression = IdentifierName($"global::{typeNamespace}.{uxmlAttributeDefaultValue}");
        return info;
    }

    private static string GetPropertyTypeIdentifier(GeneratorExecutionContext context,
        BasePropertyDeclarationSyntax property, out TypeSyntax? typeSyntax, out string? typeNamespace)
    {
        switch (property.Type)
        {
            case PredefinedTypeSyntax predefinedType:
            {
                typeSyntax = predefinedType;
                typeNamespace = default;

                var propertyTypeIdentifier = predefinedType.Keyword.Text.FirstCharToUpper();

                return $"Uxml{propertyTypeIdentifier}AttributeDescription";
            }

            case IdentifierNameSyntax customTypeSyntax:
            {
                typeSyntax = customTypeSyntax;
                typeNamespace = customTypeSyntax.GetTypeNamespace(context);

                var type = customTypeSyntax.Identifier.Text;
                var propertyTypeText = $"global::{typeNamespace}.{type}";


                return propertyTypeText == UnityColorTypeFullName
                    ? UxmlColorAttributeDescription
                    : $"UxmlEnumAttributeDescription<{propertyTypeText}>";
            }

            default:
                typeSyntax = default;
                typeNamespace = default;
                return property.Type.GetText().ToString().Trim();
        }
    }

    private static FieldDeclarationSyntax GetAttributeFieldDeclaration(UxmlAttributeInfo attributeInfo)
    {
        return FieldWidget(
            identifier: attributeInfo.PrivateFieldName,
            type: IdentifierName(string.Format(UnityUiElementsFullName, attributeInfo.TypeIdentifier)),
            modifiers: new[] { SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword },
            initializers: new[]
            {
                AssignmentWidget(
                    left: IdentifierName("name"),
                    right: LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(attributeInfo.AttributeUxmlName))),
                AssignmentWidget(
                    left: IdentifierName("defaultValue"),
                    right: attributeInfo.DefaultValueAssignmentExpression)
            },
            addGeneratedCodeAttributes: true
        );
    }

    private static IEnumerable<StatementSyntax> GetAttributeValueAssignments(GeneratorExecutionContext context,
        UxmlTraitsCapture capture)
    {
        var attributeValueAssignments = new List<StatementSyntax>();

        foreach (var (property, _) in capture.Properties)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var propertyName = property.GetName();
            var fieldName = propertyName.ToPrivateFieldName();

            attributeValueAssignments.Add(GetAttributeValueAssignmentStatement(propertyName, fieldName));
        }

        return attributeValueAssignments;
    }

    private static StatementSyntax GetAttributeValueAssignmentStatement(string propertyName, string fieldName)
    {
        return AssignmentStatementWidget(
            left: MemberAccessWidget(identifier: "control", memberName: propertyName),
            right: MethodAccessWidget(
                identifier: fieldName,
                methodName: "GetValueFromBag",
                arguments: new[]
                {
                    Argument(IdentifierName("bag")),
                    Argument(IdentifierName("context"))
                }));
    }
}