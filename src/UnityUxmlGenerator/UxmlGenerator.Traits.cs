using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using UnityUxmlGenerator.Captures;
using UnityUxmlGenerator.Diagnostics;
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
                member: ClassWidget(
                    identifier: capture.ClassName,
                    modifier: SyntaxKind.PartialKeyword,
                    members: new MemberDeclarationSyntax[]
                    {
                        ClassWidget(
                            identifier: "UxmlTraits",
                            modifiers: new[] { SyntaxKind.PublicKeyword, SyntaxKind.NewKeyword },
                            baseType: SimpleBaseType(IdentifierName($"{GetBaseClassName(context, capture)}.UxmlTraits")),
                            members: GetTraitsClassMembers(context, capture),
                            addGeneratedCodeAttributes: true),
                        MethodWidget(
                            identifier: "OnUxmlTraitsInitialized",
                            type: PredefinedType(Token(SyntaxKind.VoidKeyword)),
                            modifier: SyntaxKind.PartialKeyword,
                            parameter: ParameterWidget(
                                identifier: "uxmlAttributes",
                                type: IdentifierName(string.Format(UnityUiElementsFullName, "IUxmlAttributes"))),
                            addGeneratedCodeAttributes: true)
                    }),
                normalizeWhitespace: true)
            .GetText(Encoding.UTF8);
    }

    private static IEnumerable<MemberDeclarationSyntax> GetTraitsClassMembers(GeneratorExecutionContext context,
        UxmlTraitsCapture capture)
    {
        var initMethodBody = new List<StatementSyntax>
        {
            MethodCallWidget(
                expression: BaseExpression(),
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

        var traitsClassMembers = new List<MemberDeclarationSyntax>();

        foreach (var (property, attribute) in capture.Properties)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (IsValidAttribute(context, attribute) == false ||
                TryGetUxmlAttributeInfo(context, property, attribute, out var uxmlAttributeInfo) == false)
            {
                continue;
            }

            var propertyName = uxmlAttributeInfo.PropertyName;
            var fieldName = uxmlAttributeInfo.PrivateFieldName;

            initMethodBody.Add(GetAttributeValueAssignmentStatement(propertyName, fieldName));
            traitsClassMembers.Add(GetAttributeFieldDeclaration(uxmlAttributeInfo));
        }

        initMethodBody.Add(MethodCallWidget(
            expression: IdentifierName("control"),
            identifier: "OnUxmlTraitsInitialized",
            argument: Argument(IdentifierName("bag")))
        );

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
            bodyStatements: initMethodBody,
            addGeneratedCodeAttributes: true
        );

        traitsClassMembers.Add(initMethod);

        return traitsClassMembers;
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

    private static StatementSyntax GetAttributeValueAssignmentStatement(string propertyName, string fieldName)
    {
        return AssignmentStatementWidget(
            left: MemberAccessWidget(identifier: "control", memberName: propertyName),
            right: MethodInvocationWidget(
                memberIdentifier: fieldName,
                methodName: "GetValueFromBag",
                arguments: new[]
                {
                    Argument(IdentifierName("bag")),
                    Argument(IdentifierName("context"))
                }));
    }

    private static string GetBaseClassName(GeneratorExecutionContext context, UxmlTraitsCapture capture)
    {
        var baseClassName = capture.GetBaseClassName(out var genericTypeArguments);
        var baseClassNamespace = capture.BaseClassType.GetTypeNamespace(context);

        if (genericTypeArguments is null)
        {
            return $"global::{baseClassNamespace}.{baseClassName}";
        }

        var stringBuilder = new StringBuilder();

        stringBuilder.Append("global::");
        stringBuilder.Append(baseClassNamespace);
        stringBuilder.Append('.');
        stringBuilder.Append(baseClassName);
        stringBuilder.AppendGenericString(context, genericTypeArguments);

        return stringBuilder.ToString();
    }

    private static bool TryGetUxmlAttributeInfo(GeneratorExecutionContext context, PropertyDeclarationSyntax property,
        AttributeSyntax attribute, out UxmlAttributeInfo attributeInfo)
    {
        var propertyName = property.GetName();
        var uxmlAttributeDefaultValue = GetAttributeArgumentValue(attribute);

        attributeInfo = new UxmlAttributeInfo
        {
            PropertyName = propertyName,
            PrivateFieldName = propertyName.ToPrivateFieldName(),
            AttributeUxmlName = propertyName.ToDashCase()
        };

        if (property.Type is PredefinedTypeSyntax predefinedType)
        {
            ConfigureAttributeInfoAsPredefinedType(predefinedType, uxmlAttributeDefaultValue, ref attributeInfo);
            return true;
        }

        SyntaxToken? propertyTypeIdentifier = property.Type switch
        {
            IdentifierNameSyntax identifierName => identifierName.Identifier,
            QualifiedNameSyntax qualifiedNameSyntax => qualifiedNameSyntax.Right.Identifier,
            _ => null
        };

        if (propertyTypeIdentifier is null)
        {
            return false;
        }

        var typeSymbol = property.Type.GetTypeSymbol(context);
        var typeNamespace = typeSymbol?.ContainingNamespace.ToString();

        var typeName = propertyTypeIdentifier.Value.Text;
        var typeFullName = $"global::{typeNamespace}.{typeName}";

        if (typeFullName == UnityColorTypeFullName)
        {
            attributeInfo.TypeIdentifier = UxmlColorAttributeDescription;
            attributeInfo.DefaultValueAssignmentExpression = uxmlAttributeDefaultValue is null
                ? LiteralExpression(SyntaxKind.DefaultLiteralExpression, Token(SyntaxKind.DefaultKeyword))
                : IdentifierName($"global::UnityEngine.{uxmlAttributeDefaultValue}");
            return true;
        }

        if (typeSymbol?.TypeKind is not TypeKind.Enum)
        {
            ReportDiagnostic(PropertyTypeIsNotSupportedError, context, property.GetLocation(), typeSymbol?.Name);
            return false;
        }

        attributeInfo.TypeIdentifier = $"UxmlEnumAttributeDescription<{typeFullName}>";

        if (uxmlAttributeDefaultValue is null)
        {
            attributeInfo.DefaultValueAssignmentExpression = LiteralExpression(SyntaxKind.DefaultLiteralExpression,
                Token(SyntaxKind.DefaultKeyword));
            return true;
        }

        var attributeArgumentTypeSymbol = attribute.ArgumentList!.Arguments.First().Expression.GetTypeSymbol(context);
        if (attributeArgumentTypeSymbol?.TypeKind is TypeKind.Enum)
        {
            attributeInfo.DefaultValueAssignmentExpression =
                IdentifierName($"{typeFullName}.{uxmlAttributeDefaultValue}");
            return true;
        }

        ReportDiagnostic(IncorrectEnumDefaultValueTypeError, context, attribute.GetLocation(),
            attributeArgumentTypeSymbol?.Name);
        return false;
    }

    private static void ConfigureAttributeInfoAsPredefinedType(PredefinedTypeSyntax predefinedPropertyType,
        string? uxmlAttributeDefaultValue, ref UxmlAttributeInfo attributeInfo)
    {
        attributeInfo.TypeIdentifier = $"Uxml{predefinedPropertyType.Keyword.Text.FirstCharToUpper()}AttributeDescription";

        if (uxmlAttributeDefaultValue is null)
        {
            attributeInfo.DefaultValueAssignmentExpression =
                LiteralExpression(SyntaxKind.DefaultLiteralExpression, Token(SyntaxKind.DefaultKeyword));
            return;
        }

        if (predefinedPropertyType.IsBoolType())
        {
            attributeInfo.DefaultValueAssignmentExpression = IdentifierName(uxmlAttributeDefaultValue);
            return;
        }

        if (predefinedPropertyType.IsStringType())
        {
            attributeInfo.DefaultValueAssignmentExpression = LiteralExpression(SyntaxKind.StringLiteralExpression,
                Literal(uxmlAttributeDefaultValue));
            return;
        }

        if (predefinedPropertyType.IsNumericType())
        {
            attributeInfo.DefaultValueAssignmentExpression = LiteralExpression(SyntaxKind.NumericLiteralExpression,
                Literal(uxmlAttributeDefaultValue, uxmlAttributeDefaultValue));
        }
    }

    private static string? GetAttributeArgumentValue(AttributeSyntax attribute)
    {
        if (attribute.ArgumentList is null || attribute.ArgumentList.Arguments.Any() == false)
        {
            return null;
        }

        return attribute.ArgumentList.Arguments.First().Expression switch
        {
            IdentifierNameSyntax identifierName => identifierName.Identifier.Text,
            PrefixUnaryExpressionSyntax unary => GetUnaryExpressionValue(unary),
            LiteralExpressionSyntax literal => GetLiteralExpressionValue(literal),
            InvocationExpressionSyntax invocation => GetInvocationExpressionValue(invocation),
            MemberAccessExpressionSyntax member => GetMemberAccessExpressionValue(member),
            _ => null
        };
    }

    private static string GetUnaryExpressionValue(PrefixUnaryExpressionSyntax unary)
    {
        var value = unary.Operand.GetText().ToString();

        return unary.IsKind(SyntaxKind.UnaryMinusExpression) ? $"-{value}" : value;
    }

    private static string? GetLiteralExpressionValue(LiteralExpressionSyntax literal)
    {
        if (literal.Token.IsKind(SyntaxKind.DefaultKeyword))
        {
            return null;
        }

        return literal.Token.IsKind(SyntaxKind.StringLiteralToken) ? literal.Token.ValueText : literal.Token.Text;
    }

    private static string GetMemberAccessExpressionValue(MemberAccessExpressionSyntax member)
    {
        return member.Name.Identifier.Text;
    }

    private static string? GetInvocationExpressionValue(InvocationExpressionSyntax invocation)
    {
        return invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression.GetText().ToString();
    }

    private static void ReportDiagnostic(DiagnosticDescriptor diagnosticDescriptor, GeneratorExecutionContext context,
        Location location, params object?[]? args)
    {
        context.ReportDiagnostic(diagnosticDescriptor.CreateDiagnostic(location, args));
    }
}