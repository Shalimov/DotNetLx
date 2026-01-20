using DotNetLxInterpreter.FrontEnd;

namespace DotNetLxInterpreter.Exceptions;

public class LxParseException : Exception
{
  public Token Token { get; }

  public LxParseException(string message, Token token) : base(message)
  {
    Token = token;
  }
}
