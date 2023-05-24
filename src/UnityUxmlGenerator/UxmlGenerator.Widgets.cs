﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityUxmlGenerator;

internal sealed partial class UxmlGenerator
{
    private static CompilationUnitSyntax CompilationUnitWidget(
        string? namespaceIdentifier = null,
        MemberDeclarationSyntax? member = null,
        IEnumerable<MemberDeclarationSyntax>? members = null,
        bool addGeneratedCodeLeadingTrivia = true,
        bool normalizeWhitespace = true)
    {
        var compilationUnit = CompilationUnit();

        if (string.IsNullOrWhiteSpace(namespaceIdentifier) == false)
        {
            compilationUnit = compilationUnit
                .AddMembers(NamespaceWidget(identifier: namespaceIdentifier!, member: member, members: members));
        }
        else
        {
            if (member is not null)
            {
                compilationUnit = compilationUnit.WithMembers(compilationUnit.Members.Add(member));
            }

            if (members is not null)
            {
                compilationUnit = compilationUnit.WithMembers(List(members));
            }
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
        IEnumerable<MemberDeclarationSyntax>? members = null)
    {
        var namespaceDeclaration = NamespaceDeclaration(IdentifierName(identifier));

        if (member is not null)
        {
            namespaceDeclaration = namespaceDeclaration.WithMembers(namespaceDeclaration.Members.Add(member));
        }

        if (members is not null)
        {
            namespaceDeclaration = namespaceDeclaration.WithMembers(List(members));
        }

        return namespaceDeclaration;
    }

    private static ClassDeclarationSyntax ClassWidget(
        string identifier,
        SyntaxKind? modifier = null,
        IEnumerable<SyntaxKind>? modifiers = null,
        BaseTypeSyntax? baseType = null,
        IEnumerable<BaseTypeSyntax>? baseTypes = null,
        MemberDeclarationSyntax? member = null,
        IEnumerable<MemberDeclarationSyntax>? members = null,
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
            classDeclaration = classDeclaration.WithMembers(classDeclaration.Members.Add(member));
        }

        if (members is not null)
        {
            classDeclaration = classDeclaration.WithMembers(List(members));
        }

        return BaseWidgetDecoration(classDeclaration, modifier, modifiers, addGeneratedCodeAttributes);
    }

    private static ConstructorDeclarationSyntax ConstructorWidget(
        string identifier,
        SyntaxKind? modifier = null,
        IEnumerable<SyntaxKind>? modifiers = null,
        ParameterSyntax? parameter = null,
        IEnumerable<ParameterSyntax>? parameters = null,
        StatementSyntax? bodyStatement = null,
        IEnumerable<StatementSyntax>? bodyStatements = null,
        bool addGeneratedCodeAttributes = false)
    {
        var constructorDeclaration = ConstructorDeclaration(Identifier(identifier));

        if (parameter is not null)
        {
            constructorDeclaration = constructorDeclaration.WithParameterList(constructorDeclaration.ParameterList
                .WithParameters(constructorDeclaration.ParameterList.Parameters.Add(parameter)));
        }

        if (parameters is not null)
        {
            constructorDeclaration = constructorDeclaration.WithParameterList(constructorDeclaration.ParameterList
                .WithParameters(constructorDeclaration.ParameterList.Parameters.AddRange(parameters)));
        }

        if (bodyStatement is not null)
        {
            constructorDeclaration = constructorDeclaration.WithBody(Block(SingletonList(bodyStatement)));
        }

        if (bodyStatements is not null)
        {
            constructorDeclaration = constructorDeclaration.WithBody(Block(bodyStatements));
        }

        return BaseWidgetDecoration(constructorDeclaration, modifier, modifiers, addGeneratedCodeAttributes);
    }

    private static FieldDeclarationSyntax FieldWidget(
        string identifier,
        TypeSyntax type,
        SyntaxKind? modifier = null,
        IEnumerable<SyntaxKind>? modifiers = null,
        ExpressionSyntax? initializer = null,
        IEnumerable<ExpressionSyntax>? initializers = null,
        bool addGeneratedCodeAttributes = false)
    {
        var variableDeclaration = VariableDeclarator(Identifier(identifier));

        if (initializer is not null)
        {
            variableDeclaration = variableDeclaration.WithInitializer(EqualsValueClause(ImplicitObjectCreationExpression()
                .WithInitializer(InitializerExpression(SyntaxKind.ObjectInitializerExpression, SingletonSeparatedList(initializer)))));
        }

        if (initializers is not null)
        {
            variableDeclaration = variableDeclaration.WithInitializer(EqualsValueClause(ImplicitObjectCreationExpression()
                    .WithInitializer(InitializerExpression(SyntaxKind.ObjectInitializerExpression, SeparatedList(initializers)))));
        }

        var fieldDeclaration = FieldDeclaration(VariableDeclaration(type)
            .WithVariables(SingletonSeparatedList(variableDeclaration)));

        return BaseWidgetDecoration(fieldDeclaration, modifier, modifiers, addGeneratedCodeAttributes);
    }

    private static PropertyDeclarationSyntax PropertyWidget(
        string identifier,
        TypeSyntax type,
        SyntaxKind? modifier = null,
        IEnumerable<SyntaxKind>? modifiers = null,
        SyntaxKind? accessor = null,
        IEnumerable<SyntaxKind>? accessors = null,
        bool addGeneratedCodeAttributes = false)
    {
        var propertyDeclaration = PropertyDeclaration(type, Identifier(identifier));

        if (accessor is not null)
        {
            var accessorList = propertyDeclaration.AccessorList ?? AccessorList();

            propertyDeclaration = propertyDeclaration.WithAccessorList(accessorList
                .WithAccessors(accessorList.Accessors.Add(AccessorWidget(accessor.Value))));
        }

        if (accessors is not null)
        {
            var accessorList = propertyDeclaration.AccessorList ?? AccessorList();

            propertyDeclaration = propertyDeclaration.WithAccessorList(accessorList
                .WithAccessors(accessorList.Accessors.AddRange(accessors.Select(AccessorWidget))));
        }

        return BaseWidgetDecoration(propertyDeclaration, modifier, modifiers, addGeneratedCodeAttributes);
    }

    private static MethodDeclarationSyntax MethodWidget(
        string identifier,
        TypeSyntax type,
        SyntaxKind? modifier = null,
        IEnumerable<SyntaxKind>? modifiers = null,
        ParameterSyntax? parameter = null,
        IEnumerable<ParameterSyntax>? parameters = null,
        StatementSyntax? bodyStatement = null,
        IEnumerable<StatementSyntax>? bodyStatements = null,
        bool addGeneratedCodeAttributes = false)
    {
        var methodDeclaration = MethodDeclaration(type, Identifier(identifier));

        if (parameter is not null)
        {
            methodDeclaration = methodDeclaration.WithParameterList(methodDeclaration.ParameterList
                .WithParameters(methodDeclaration.ParameterList.Parameters.Add(parameter)));
        }

        if (parameters is not null)
        {
            methodDeclaration = methodDeclaration.WithParameterList(methodDeclaration.ParameterList
                .WithParameters(methodDeclaration.ParameterList.Parameters.AddRange(parameters)));
        }

        if (bodyStatement is not null)
        {
            methodDeclaration = methodDeclaration.WithBody(Block(SingletonList(bodyStatement)));
        }

        if (bodyStatements is not null)
        {
            methodDeclaration = methodDeclaration.WithBody(Block(bodyStatements));
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
        ArgumentSyntax? argument = null,
        IEnumerable<ArgumentSyntax>? arguments = null)
    {
        var baseExpression = InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
            BaseExpression(), IdentifierName(identifier)));

        if (argument is not null)
        {
            baseExpression = baseExpression.WithArgumentList(baseExpression.ArgumentList
                .WithArguments(baseExpression.ArgumentList.Arguments.Add(argument)));
        }

        if (arguments is not null)
        {
            baseExpression = baseExpression.WithArgumentList(baseExpression.ArgumentList
                .WithArguments(baseExpression.ArgumentList.Arguments.AddRange(arguments)));
        }

        return ExpressionStatement(baseExpression);
    }

    private static StatementSyntax LocalVariableWidget(
        string identifier,
        EqualsValueClauseSyntax? initializer)
    {
        return
            LocalDeclarationStatement(
                VariableDeclaration(IdentifierName(Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList())))
                    .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(identifier))
                            .WithInitializer(initializer))));
    }

    private static EqualsValueClauseSyntax CastExpressionWidget(
        string identifier,
        TypeSyntax typeToCast)
    {
        return EqualsValueClause(CastExpression(typeToCast, IdentifierName(identifier)));
    }

    private static StatementSyntax AssignmentStatementWidget(
        ExpressionSyntax left,
        ExpressionSyntax right,
        SyntaxKind expression = SyntaxKind.SimpleAssignmentExpression)
    {
        return ExpressionStatement(AssignmentWidget(left, right, expression));
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

    private static ExpressionSyntax MethodInvocationWidget(
        string memberIdentifier,
        string methodName,
        ArgumentSyntax? argument = null,
        IEnumerable<ArgumentSyntax>? arguments = null)
    {
        var invocationSyntax =
            InvocationExpression(MemberAccessWidget(identifier: memberIdentifier, memberName: methodName));

        if (argument is not null)
        {
            invocationSyntax = invocationSyntax.WithArgumentList(invocationSyntax.ArgumentList
                .WithArguments(invocationSyntax.ArgumentList.Arguments.Add(argument)));
        }

        if (arguments is not null)
        {
            invocationSyntax = invocationSyntax.WithArgumentList(invocationSyntax.ArgumentList
                .WithArguments(invocationSyntax.ArgumentList.Arguments.AddRange(arguments)));
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
        IEnumerable<SyntaxKind>? modifiers,
        bool addGeneratedCodeAttributes) where TSyntax : MemberDeclarationSyntax
    {
        if (modifier is not null)
        {
            widget = (TSyntax) widget.WithModifiers(widget.Modifiers.Add(Token(modifier.Value)));
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