using DotNetLoxInterpreter.FrontEnd;

namespace DotNetLoxInterpreter.Exceptions;

public class LxParseException : Exception
{
  public Token Token { get; }

  public LxParseException(string message, Token token) : base(message)
  {
    Token = token;
  }
}
