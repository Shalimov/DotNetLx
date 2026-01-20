namespace DotNetLxInterpreter.FrontEnd;

public class Token(TokenType tokenType, string lexeme, object? literal, int line, int column)
{
    public TokenType Type { get; } = tokenType;
    public string Lexeme { get; } = lexeme;
    public object? Literal { get; } = literal;
    public int Line { get; } = line;
    public int Column { get; } = column;

    public override int GetHashCode() => Lexeme.GetHashCode();

    public override bool Equals(object? obj)
    {
        if (obj is Token other)
        {
            return string.Equals(Lexeme, other.Lexeme);
        }

        return false;
    }

    // Overload == and != for consistency
    public static bool operator ==(Token left, Token right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Token left, Token right) => !(left == right);

    public override string ToString()
    {
        return Literal is not null ? $"{Type}: {Lexeme} {Literal}" : $"{Type}: {Lexeme}";
    }
}