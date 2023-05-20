using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace UnityUxmlGenerator;

internal sealed partial class UxmlGenerator
{
    private const string UxmlElementClassName = "UxmlElementAttribute";
    private const string UxmlAttributeClassName = "UxmlAttributeAttribute";

    private static readonly AssemblyName AssemblyName = typeof(UxmlGenerator).Assembly.GetName();

    private static SourceText GenerateUxmlElementAttribute()
    {
        var baseList = SimpleBaseType(IdentifierName("global::System.Attribute"));

        var @class = ClassDeclaration(UxmlElementClassName)
            .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.SealedKeyword)))
            .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(baseList)));

        return GetCompilationUnit((TypeDeclarationSyntax) ProcessMemberDeclaration(@class), AssemblyName.Name)
            .GetText(Encoding.UTF8);
    }

    private static SourceText GenerateUxmlAttributeAttribute()
    {
        var baseList = SimpleBaseType(IdentifierName("global::System.Attribute"));

        var @class = ClassDeclaration(UxmlAttributeClassName)
            .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.SealedKeyword)))
            .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(baseList)));

        var members = GetUxmlAttributeMembers();

        return GetCompilationUnit((TypeDeclarationSyntax) ProcessMemberDeclaration(@class), AssemblyName.Name, members)
            .GetText(Encoding.UTF8);
    }

    private static MemberDeclarationSyntax[] GetUxmlAttributeMembers()
    {
        var classConstructor =
            ConstructorDeclaration(Identifier(UxmlAttributeClassName))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier("defaultValue"))
                    .WithType(NullableType(PredefinedType(Token(SyntaxKind.ObjectKeyword))))
                    .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.DefaultLiteralExpression, Token(SyntaxKind.DefaultKeyword)))))))
                .WithBody(Block(SingletonList<StatementSyntax>(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName("DefaultValue"),
                    IdentifierName("defaultValue"))))));

        var defaultProperty =
            PropertyDeclaration(NullableType(PredefinedType(Token(SyntaxKind.ObjectKeyword))), Identifier("DefaultValue"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(AccessorList(SingletonList(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))));

        return new MemberDeclarationSyntax[] { classConstructor, defaultProperty };
    }
}