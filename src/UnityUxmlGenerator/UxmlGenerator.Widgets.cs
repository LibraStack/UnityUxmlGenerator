using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator;

internal sealed partial class UxmlGenerator
{
    private static CompilationUnitSyntax CompilationUnitWidget(
        bool addGeneratedCodeLeadingTrivia = true,
        bool normalizeWhitespace = true,
        params MemberDeclarationSyntax[]? members)
    {
        var compilationUnit = CompilationUnit();

        if (members is not null)
        {
            compilationUnit = compilationUnit.AddMembers(members);
        }

        if (addGeneratedCodeLeadingTrivia)
        {
            compilationUnit = GeneratedCodeLeadingTrivia(compilationUnit);
        }

        return normalizeWhitespace
            ? compilationUnit.NormalizeWhitespace()
            : compilationUnit;
    }

    private static NamespaceDeclarationSyntax NamespaceWidget(
        string identifier,
        MemberDeclarationSyntax? member = null,
        MemberDeclarationSyntax[]? members = null)
    {
        var namespaceDeclaration = NamespaceDeclaration(IdentifierName(identifier));

        if (member is not null)
        {
            namespaceDeclaration = namespaceDeclaration.AddMembers(member);
        }

        if (members is not null)
        {
            namespaceDeclaration = namespaceDeclaration.AddMembers(members);
        }

        return namespaceDeclaration;
    }

    private static ClassDeclarationSyntax ClassWidget(
        string identifier,
        SyntaxKind? modifier = null,
        SyntaxKind[]? modifiers = null,
        BaseTypeSyntax? baseType = null,
        BaseTypeSyntax[]? baseTypes = null,
        MemberDeclarationSyntax? member = null,
        MemberDeclarationSyntax[]? members = null,
        bool addGeneratedCodeAttributes = false)
    {
        var classDeclaration = ClassDeclaration(identifier);

        if (baseType is not null)
        {
            classDeclaration = classDeclaration.WithBaseList(BaseList(SingletonSeparatedList(baseType)));
        }

        if (baseTypes is not null)
        {
            classDeclaration = classDeclaration.WithBaseList(BaseList(SeparatedList(baseTypes)));
        }

        if (member is not null)
        {
            classDeclaration = classDeclaration.AddMembers(member);
        }

        if (members is not null)
        {
            classDeclaration = classDeclaration.AddMembers(members);
        }

        return BaseWidgetDecoration(classDeclaration, modifier, modifiers, addGeneratedCodeAttributes);
    }

    private static ConstructorDeclarationSyntax ConstructorWidget(
        string identifier,
        SyntaxKind? modifier = null,
        SyntaxKind[]? modifiers = null,
        ParameterSyntax? parameter = null,
        ParameterSyntax[]? parameters = null,
        bool addGeneratedCodeAttributes = false,
        params StatementSyntax[]? body)
    {
        var constructorDeclaration = ConstructorDeclaration(Identifier(identifier));

        if (parameter is not null)
        {
            constructorDeclaration = constructorDeclaration.AddParameterListParameters(parameter);
        }

        if (parameters is not null)
        {
            constructorDeclaration = constructorDeclaration.AddParameterListParameters(parameters);
        }

        if (body is not null)
        {
            constructorDeclaration = constructorDeclaration.WithBody(Block(body));
        }

        return BaseWidgetDecoration(constructorDeclaration, modifier, modifiers, addGeneratedCodeAttributes);
    }

    private static FieldDeclarationSyntax FieldWidget(
        string identifier,
        TypeSyntax type,
        SyntaxKind? modifier = null,
        SyntaxKind[]? modifiers = null,
        bool addGeneratedCodeAttributes = false,
        params ExpressionSyntax[]? initializers)
    {
        var variableDeclaration = VariableDeclarator(Identifier(identifier))
            .WithInitializer(EqualsValueClause(ImplicitObjectCreationExpression()
                .WithInitializer(InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                    SeparatedList(initializers)))));

        var fieldDeclaration = FieldDeclaration(VariableDeclaration(type)
            .WithVariables(SingletonSeparatedList(variableDeclaration)));

        return BaseWidgetDecoration(fieldDeclaration, modifier, modifiers, addGeneratedCodeAttributes);
    }

    private static PropertyDeclarationSyntax PropertyWidget(
        string identifier,
        TypeSyntax type,
        SyntaxKind? modifier = null,
        SyntaxKind[]? modifiers = null,
        bool addGeneratedCodeAttributes = false,
        params SyntaxKind[]? accessor)
    {
        var propertyDeclaration = PropertyDeclaration(type, Identifier(identifier));

        if (accessor is not null)
        {
            propertyDeclaration =
                propertyDeclaration.AddAccessorListAccessors(accessor.Select(AccessorWidget).ToArray());
        }

        return BaseWidgetDecoration(propertyDeclaration, modifier, modifiers, addGeneratedCodeAttributes);
    }

    private static MethodDeclarationSyntax MethodWidget(
        string identifier,
        TypeSyntax type,
        SyntaxKind? modifier = null,
        SyntaxKind[]? modifiers = null,
        ParameterSyntax? parameter = null,
        ParameterSyntax[]? parameters = null,
        bool addGeneratedCodeAttributes = false,
        params StatementSyntax[]? body)
    {
        var methodDeclaration = MethodDeclaration(type, Identifier(identifier));

        if (parameter is not null)
        {
            methodDeclaration = methodDeclaration.AddParameterListParameters(parameter);
        }

        if (parameters is not null)
        {
            methodDeclaration = methodDeclaration.AddParameterListParameters(parameters);
        }

        if (body is not null)
        {
            methodDeclaration = methodDeclaration.WithBody(Block(body));
        }

        return BaseWidgetDecoration(methodDeclaration, modifier, modifiers, addGeneratedCodeAttributes);
    }

    private static ParameterSyntax ParameterWidget(
        string identifier,
        TypeSyntax type,
        bool addDefaultKeyword = false)
    {
        var parameterSyntax = Parameter(Identifier(identifier))
            .WithType(type);

        if (addDefaultKeyword)
        {
            parameterSyntax = parameterSyntax.WithDefault(EqualsValueClause(
                LiteralExpression(SyntaxKind.DefaultLiteralExpression, Token(SyntaxKind.DefaultKeyword))));
        }

        return parameterSyntax;
    }

    private static StatementSyntax MethodBaseCallWidget(
        string identifier,
        params ArgumentSyntax[]? arguments)
    {
        var baseExpression = InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
            BaseExpression(), IdentifierName(identifier)));

        if (arguments is not null)
        {
            baseExpression = baseExpression.AddArgumentListArguments(arguments);
        }

        return ExpressionStatement(baseExpression);
    }

    private static StatementSyntax LocalVariableWidget(
        string identifier,
        EqualsValueClauseSyntax? initializer)
    {
        return
            LocalDeclarationStatement(
                VariableDeclaration(IdentifierName(Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var",
                        TriviaList())))
                    .WithVariables(
                        SingletonSeparatedList(VariableDeclarator(Identifier(identifier))
                            .WithInitializer(initializer))));
    }

    private static EqualsValueClauseSyntax CastExpressionWidget(
        string identifier,
        TypeSyntax typeToCast)
    {
        return EqualsValueClause(CastExpression(typeToCast, IdentifierName(identifier)));
    }

    private static ExpressionSyntax AssignmentWidget(
        ExpressionSyntax left,
        ExpressionSyntax right,
        SyntaxKind expression = SyntaxKind.SimpleAssignmentExpression)
    {
        return AssignmentExpression(expression, left, right);
    }

    private static ExpressionSyntax MemberAccessWidget(
        string identifier,
        string memberName)
    {
        return MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(identifier),
            IdentifierName(memberName));
    }

    private static ExpressionSyntax MethodAccessWidget(
        string identifier,
        string methodName,
        params ArgumentSyntax[]? arguments)
    {
        var invocationSyntax =
            InvocationExpression(MemberAccessWidget(identifier: identifier, memberName: methodName));

        if (arguments is not null)
        {
            invocationSyntax = invocationSyntax.AddArgumentListArguments(arguments);
        }

        return invocationSyntax;
    }

    private static AccessorDeclarationSyntax AccessorWidget(SyntaxKind kind)
    {
        return AccessorDeclaration(kind).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }
    
    private static TSyntax BaseWidgetDecoration<TSyntax>(
        TSyntax widget,
        SyntaxKind? modifier,
        SyntaxKind[]? modifiers,
        bool addGeneratedCodeAttributes) where TSyntax : MemberDeclarationSyntax
    {
        if (modifier is not null)
        {
            widget = (TSyntax) widget.AddModifiers(Token(modifier.Value));
        }

        if (modifiers is not null)
        {
            widget = (TSyntax) widget.WithModifiers(TokenList(modifiers.Select(Token)));
        }

        return addGeneratedCodeAttributes ? GeneratedCodeAttributesWidget(widget) : widget;
    }

    private static TSyntax GeneratedCodeLeadingTrivia<TSyntax>(TSyntax node) where TSyntax : SyntaxNode
    {
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

        return node.WithLeadingTrivia(syntaxTriviaList);
    }

    private static TSyntax GeneratedCodeAttributesWidget<TSyntax>(TSyntax member) where TSyntax : MemberDeclarationSyntax
    {
        // [GeneratedCode] is always present.
        member = (TSyntax) member
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
            member = (TSyntax) member
                .AddAttributeLists(AttributeList(SingletonSeparatedList(
                    Attribute(IdentifierName("global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage")))));
        }

        return member;
    }
}