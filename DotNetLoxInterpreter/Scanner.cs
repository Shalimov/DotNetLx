namespace DotNetLoxInterpreter;

public class Scanner(string source)
{
    private static readonly Dictionary<string, TokenType> ReservedIdentifiers = new()
    {
        { "fun", TokenType.FUN },
        { "return", TokenType.RETURN },
        { "super", TokenType.SUPER },
        { "print", TokenType.PRINT },
        { "class", TokenType.CLASS },
        { "this", TokenType.THIS },
        { "nil", TokenType.NIL },
        { "for", TokenType.FOR },
        { "while", TokenType.WHILE },
        { "if", TokenType.IF },
        { "else", TokenType.ELSE },
        { "and", TokenType.AND },
        { "or", TokenType.OR },
        { "false", TokenType.FALSE },
        { "true", TokenType.TRUE },
        { "var", TokenType.VAR }
    };

    private readonly string _source = source;
    private readonly List<Token> _tokens = new();
    private int _columnStartOffset;
    private int _current;
    private int _line;

    private int _start;

    public List<Token> Scan()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.EOF, string.Empty, null, _line, _start));

        return _tokens;
    }

    private void ScanToken()
    {
        var currentChar = Advance();

        switch (currentChar)
        {
            case '(': AddToken(TokenType.LEFT_PAREN); break;
            case ')': AddToken(TokenType.RIGHT_PAREN); break;
            case '{': AddToken(TokenType.LEFT_BRACE); break;
            case '}': AddToken(TokenType.RIGHT_BRACE); break;
            case ',': AddToken(TokenType.COMMA); break;
            case ';': AddToken(TokenType.SEMICOLON); break;
            case '.': AddToken(TokenType.DOT); break;
            case '-': AddToken(TokenType.MINUS); break;
            case '+': AddToken(TokenType.PLUS); break;
            case '*': AddToken(TokenType.STAR); break;

            case '"':
                ExtractStringLiteral();
                break;

            case '/':
                if (Peek() is '/' or '*') SkipComment();
                else AddToken(TokenType.SLASH);

                break;
            case '!':
                AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                break;
            case '|':
                if (Match('>')) AddToken(TokenType.PIPELINE);
                break;

            case >= '0' and <= '9':
                ExtractNumberLiteral();
                break;

            case ' ' or '\t' or '\r':
                break;

            case '\n':
                _line++;
                _columnStartOffset = _current;
                break;

            default:
                if (IsAlpha(currentChar))
                {
                    ExtractIdentifier();

                    break;
                }

                DotnetLox.ReportError(_line, _current - _columnStartOffset, $"Unrecognized character '{currentChar}'");
                break;
        }
    }

    private char Advance()
    {
        return _source[_current++];
    }

    private char Peek()
    {
        return IsAtEnd() ? '\0' : _source[_current];
    }

    private char PeekNext()
    {
        return _current + 1 >= _source.Length ? '\0' : _source[_current + 1];
    }

    private bool Match(char character)
    {
        if (IsAtEnd()) return false;
        if (_source[_current] != character) return true;

        _current++;
        return true;
    }

    private void ExtractNumberLiteral()
    {
        while (char.IsDigit(Peek())) Advance();

        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            Advance();

            while (char.IsDigit(Peek())) Advance();
        }

        var numberLiteral = _source.Substring(_start, _current - _start);

        if (double.TryParse(numberLiteral, out var number)) AddToken(TokenType.NUMBER, number);
    }

    private void ExtractStringLiteral()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n')
            {
                _line++;
                _columnStartOffset = _current;
            }

            Advance();
        }

        if (IsAtEnd())
        {
            DotnetLox.ReportError(_line, _current - _columnStartOffset, "Unterminated string.");

            return;
        }

        // Close string here
        Advance();

        // Take the literal without quotes
        AddToken(TokenType.STRING, _source.Substring(_start + 1, _current - _start));
    }

    private void ExtractIdentifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();

        var identifier = _source.Substring(_start, _current - _start);

        if (ReservedIdentifiers.TryGetValue(identifier, out var reservedIdentifier))
            AddToken(reservedIdentifier, identifier);
        else
            AddToken(TokenType.IDENTIFIER, identifier);
    }

    private void SkipComment()
    {
        var singleLine = Advance() == '/';

        if (singleLine)
        {
            // The reason of using peek instead of Match or even Advance in the condition block
            // is to avoid skipping a new line
            // cuz we want to keep counting lines for the reporting purposes
            while (Peek() != '\n' && !IsAtEnd()) Advance();

            return;
        }

        // Handling of multiline
        byte nestedness = 1;

        do
        {
            var currentChar = Advance();

            if (currentChar == '*' && Peek() == '/')
            {
                nestedness--;
                Advance();
            }
            else if (currentChar == '/' && Peek() == '*')
            {
                nestedness++;
                Advance();
            }
            else if (currentChar == '\n')
            {
                _line++;
            }
        } while (nestedness > 0 && !IsAtEnd());
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, object? literal)
    {
        _tokens.Add(new Token(type, _source.Substring(_start, _current - _start), literal, _line, _start));
    }

    private bool IsDigit(char character)
    {
        return character is >= '0' and <= '9';
    }

    private bool IsAlpha(char character)
    {
        return character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
    }

    private bool IsAlphaNumeric(char character)
    {
        return IsAlpha(character) || IsDigit(character);
    }

    private bool IsAtEnd()
    {
        return _current >= _source.Length;
    }
}