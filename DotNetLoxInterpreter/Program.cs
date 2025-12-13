using System.Text;
using DotNetLoxInterpreter.LitterVisitors;

namespace DotNetLoxInterpreter;

public static class DotnetLox
{
    private const int ExWrongUsage = 64;
    private const int ExData = 65;

    private static bool HasError { get; set; }

    public static int Main(params string[] args)
    {
        if (args.Length > 1)
        {
            // Console.WriteLine("Usage: dn-lox [script]");

            ReportError(0, 0, "Usage: dn-lox [script]");

            return ExWrongUsage;
        }

        if (args.Length == 1)
        {
            RunFile(args[0]);

            if (HasError) return ExData;
        }
        else
        {
            RunPrompt();
        }

        return 0;
    }

    private static void RunFile(string path)
    {
        var script = File.ReadAllText(path, Encoding.UTF8);
        Run(script);
    }

    private static void RunPrompt()
    {
        do
        {
            Console.Write("[script]> ");
            var scriptLine = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(scriptLine)) break;

            Run(scriptLine);

            HasError = false;
        } while (true);
    }

    private static void Run(string script)
    {
        var scanner = new Scanner(script);
        var tokens = scanner.Scan();

        var parser = new Parser(tokens.ToArray());
        var parsedExpression = parser.Parse();

        var astPrinter = new AstPrinter();

        Console.WriteLine(astPrinter.Print(parsedExpression));
    }

    public static void ReportError(int line, int col, string message)
    {
        ReportError(line, col, message, "");
    }

    public static void ReportError(Token token, string message)
    {
        if (token.Type == TokenType.EOF)
        {
            ReportError(token.Line, token.Column, message, " at end");
        }
        else
        {
            ReportError(token.Line, token.Column, message, $"at '{token.Lexeme}'");
        }
    }

    private static void ReportError(int line, int col, string message, string where)
    {

        var report = string.IsNullOrEmpty(where)
            ? $"[line {line}; col {col}] Error: {message}"
            : $"[line {line}; col {col}] Error {where}: {message}";
        Console.WriteLine(report);
    }
}