using DotNetLoxTools;

if (args.Length == 0 || args.Length > 1)
{
    Console.Error.WriteLine("Usage: DotNetLoxTools <output>");

    return 64;
}

var output = args[0];

File.WriteAllText(
    output,
    Generator.DeclareAst("Expr", [
        "Binary : Expr left, Token op, Expr right",
        "Grouping : Expr expr",
        "Literal : Object value",
        "Unary : Token op, Expr right"
    ])
);

// Console.WriteLine(
//     Generator.DeclareAst("Expr", [
//         "Binary : Expr left, Token op, Expr right",
//         "Grouping : Expr expr",
//         "Literal : Object value",
//         "Unary : Token op, Expr right"
//     ])
// );

return 0;