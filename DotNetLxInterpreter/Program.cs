using System.Text;
using DotNetLxInterpreter.FrontEnd;
using DotNetLxInterpreter.Interpretation;
using DotNetLxInterpreter.MiddleGround;
using DotNetLxInterpreter.Exceptions;

namespace DotNetLxInterpreter;

public static class DotNetLx
{
    private const int ExWrongUsage = 64;
    private const int ExData = 65;
    private const int ExSoftware = 70;

    private static ConsoleHistory _consoleHistory = default!;
    private static IInterpreter _interpreter = default!;
    private static bool HasError { get; set; }
    private static bool HasRuntimeError { get; set; }

    public static int Main(params string[] args)
    {
        if (args.Length > 1)
        {
            ReportError(0, 0, "Usage: dn-lx [script]");

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
            _consoleHistory = new ConsoleHistory("[script]> ") { MaxHistory = 10 };

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
        Console.WriteLine("Console with history (use ↑/↓ arrows). Type ':exit' to quit.");

        do
        {
            var scriptLine = _consoleHistory.ReadLine();

            var result = TryRunCommand(scriptLine);

            // 1 means command recognized
            // -1 exit command 
            if (result == 1) continue;
            if (result == -1) break;

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

        if (HasError) return;

        var staticAnalyzer = new StaticAnalyzer(_interpreter);
        staticAnalyzer.Analyze(statements);

        if (HasError) return;

        _interpreter.Interpret(statements);
    }

    private static int TryRunCommand(string command)
    {
        if (command.StartsWith(":import"))
        {
            var path = command.Substring(":import".Length).Trim();

            try
            {
                var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), path);
                var script = File.ReadAllText(scriptPath);
                Run(script);
            }
            catch
            {
                Console.WriteLine($"{command} has incorrect path");
            }

            return 1;
        }
        else if (command.Equals(":clear"))
        {
            Console.Clear();
            return 1;
        }
        else if (command.Equals(":exit"))
        {
            return -1;   
        }

        return 0;
    }

    public static void RuntimeError(LxRuntimeException exception)
    {
        Console.WriteLine("[line {line}; col {col}] Error: {message}", exception.Token.Line, exception.Token.Column, exception.Message);

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

        HasError = true;
    }
}