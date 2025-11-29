namespace DotNetLoxInterpreter;

public class Token(TokenType tokenType, string lexeme, object? literal, int line, int column)
{
    public TokenType Type { get; } = tokenType;
    public string Lexeme { get; } = lexeme;
    public object? Literal { get; } = literal;
    public int Line { get; } = line;
    public int Column { get; } = column;

    public override string ToString()
    {
        return Literal is not null ? $"{Type}: {Lexeme} {Literal}" : $"{Type}: {Lexeme}";
    }
}