using DotNetLoxTools;

if (args.Length == 0 || args.Length > 1)
{
    Console.Error.WriteLine("Usage: DotNetLoxTools <output>");

    return 64;
}

var output = args[0];

Generator.DeclareAstInFile(output, "Expr", [
        "Ternary    : Expr left, Token op1, Expr mid, Token op2, Expr right",
        "Binary     : Expr left, Token op, Expr right",
        "Logical    : Expr left, Token op, Expr right",
        "Grouping   : Expr expr",
        "Literal    : Object value",
        "Unary      : Token op, Expr right",
        "Variable   : Token name",
        "Assign     : Token name, Expr value"
    ]);

Generator.DeclareAstInFile(output, "Stmt", [
        "Block      : List<Stmt> statements",
        "If         : Expr condition, Stmt thenBranch, Stmt? elseBranch",
        "Expression : Expr expr",
        "Print      : Expr value",
        "Var        : Token name, Expr initializer"
    ]);

return 0;