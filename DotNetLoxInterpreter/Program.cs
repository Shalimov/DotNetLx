using System.Text;
using DotNetLoxInterpreter.Exceptions;
using DotNetLoxInterpreter.StatycAnalyzers;

namespace DotNetLoxInterpreter;

public static class DotnetLox
{
    private const int ExWrongUsage = 64;
    private const int ExData = 65;
    private const int ExSoftware = 70;

    private static IInterpreter _interpreter = default!;
    private static bool HasError { get; set; }
    private static bool HasRuntimeError { get; set;}

    public static int Main(params string[] args)
    {
        if (args.Length > 1)
        {
            ReportError(0, 0, "Usage: dn-lox [script]");

            return ExWrongUsage;
        }

        if (args.Length == 1)
        {
            _interpreter = new Interpreter();

            RunFile(args[0]);

            if (HasError) return ExData;
            if (HasRuntimeError) return ExSoftware;
        }
        else
        {
            _interpreter = new InterpreterRepl();

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
            HasRuntimeError = false;
        } while (true);
    }

    private static void Run(string script)
    {
        var scanner = new Scanner(script);
        var tokens = scanner.Scan();

        var parser = new Parser(tokens.ToArray());
        var statements = parser.Parse();
        
        var staticAnalyzer = new StaticAnalyzer(_interpreter);
        staticAnalyzer.Analyze(statements);

        if (HasError) return;

        _interpreter.Interpret(statements);
    }

    public static void RuntimeError(LxRuntimeException exception)
    {
        ReportError(exception.Token.Line, exception.Token.Column, exception.Message);

        HasRuntimeError = true;
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