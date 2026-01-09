using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DotNetLoxTools;

public static class Generator
{
    private const string PropertySkipIdentifier = "Void";

    private class ParsedDefinition
    {
        public class ParsedProperty
        {
            public required string Name { get; set; }
            public required string Type { get; set; }
        }

        public required string ClassName { get; set; }
        public required IEnumerable<ParsedProperty> Properties { get; set; }
    }

    public static void DeclareAstInFile(string basePath, string baseClass, string[] definitions)
    {
        var output = Path.Combine(basePath, Path.ChangeExtension(baseClass, "cs"));

        File.WriteAllText(output, DeclareAst(baseClass, definitions));
    }

    public static string DeclareAst(string baseClass, string[] definitions)
    {
        var compilationUnit = CompilationUnit();

        compilationUnit = compilationUnit.AddMembers(
            NamespaceDeclaration(ParseName("DotNetLoxInterpreter"))
                .AddMembers(CreateExprClassDeclaration(baseClass, definitions))
                .WithLeadingTrivia(Comment("/* IMPORTANT: Generated Code Do Not Amend */")));

        return compilationUnit.NormalizeWhitespace().ToFullString();
    }

    private static ClassDeclarationSyntax CreateExprClassDeclaration(string className, string[] subClassDefinitions)
    {
        var classDeclaration = ClassDeclaration(className);

        var subCls = ParseDef(subClassDefinitions);

        return classDeclaration
            .AddModifiers(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.AbstractKeyword))
            .AddMembers(CreateVisitorInterface(className, subCls))
            .AddMembers(subCls.Select(x => CreateSubClass(x, className)).ToArray<MemberDeclarationSyntax>())
            .AddMembers(
                MethodDeclaration(ParseTypeName("TR"), "Accept")
                    .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AbstractKeyword))
                    .AddParameterListParameters(Parameter(Identifier("visitor")).WithType(ParseTypeName($"IVisitor{className}<TR>")))
                    .AddTypeParameterListParameters(TypeParameter("TR"))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                );
    }


    private static InterfaceDeclarationSyntax CreateVisitorInterface(string baseClass, IEnumerable<ParsedDefinition> definitions)
    {
        var visitorMethodOverloads = definitions.Select(def => MethodDeclaration(ParseTypeName("TR"), "Visit")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(Parameter(Identifier("expr")).WithType(ParseTypeName(def.ClassName)))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))).ToArray<MemberDeclarationSyntax>();

        return InterfaceDeclaration($"IVisitor{baseClass}")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddTypeParameterListParameters(TypeParameter("TR").WithVarianceKeyword(Token(SyntaxKind.OutKeyword)))
            .AddMembers(visitorMethodOverloads);
    }

    private static ClassDeclarationSyntax CreateSubClass(ParsedDefinition definition, string baseClassName)
    {
        var subClassName = definition.ClassName;

        var properties = definition.Properties.Select(prop => CreateProperty(prop.Type, prop.Name)).ToArray();

        var constructor = CreateConstructor(subClassName, definition.Properties);

        return ClassDeclaration(subClassName)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(SimpleBaseType(ParseTypeName(baseClassName)))
            .AddMembers(properties)
            .AddMembers(constructor)
            .AddMembers(MethodDeclaration(ParseTypeName("TR"), "Accept")
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword))
                .AddParameterListParameters(Parameter(Identifier("visitor")).WithType(ParseTypeName($"IVisitor{baseClassName}<TR>")))
                .AddTypeParameterListParameters(TypeParameter("TR"))
                .WithBody(Block(
                    ReturnStatement(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("visitor"),
                                IdentifierName("Visit")
                            )
                        ).AddArgumentListArguments(
                            Argument(ThisExpression())
                        )
                    )
                ))
            );
    }

    private static PropertyDeclarationSyntax CreateProperty(string type, string name)
    {
        return PropertyDeclaration(ParseTypeName(type), Identifier(Capitalize(name)))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            );
    }

    private static ConstructorDeclarationSyntax CreateConstructor(string className,
        IEnumerable<ParsedDefinition.ParsedProperty> properties)
    {
        // Create constructor parameters
        var parameters = properties.Select(prop =>
            Parameter(Identifier(prop.Name.ToLowerInvariant()))
                .WithType(ParseTypeName(prop.Type))
        ).ToArray();

        // Create assignment statements in constructor body
        var assignments = properties.Select(prop =>
            ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(Capitalize(prop.Name)),
                    IdentifierName(prop.Name.ToLowerInvariant())
                )
            )
        ).ToArray<StatementSyntax>();

        return ConstructorDeclaration(Identifier(className))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(parameters)
            .WithBody(Block(assignments));
    }

    private static IEnumerable<ParsedDefinition> ParseDef(string[] subClassDefinitions)
    {
        return subClassDefinitions.Select(def =>
        {
            var defParts = def.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            return new ParsedDefinition()
            {
                ClassName = defParts[0],
                Properties = defParts[1]
                    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Where(prop => prop != PropertySkipIdentifier)
                    .Select(prop =>
                    {
                        var propDef = prop.Split(' ',
                            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                        return new ParsedDefinition.ParsedProperty()
                        {
                            Type = propDef[0],
                            Name = propDef[1]
                        };
                    })
            };
        });
    }

    private static string Capitalize(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return char.ToUpperInvariant(text[0]) + text.Substring(1);
    }
}