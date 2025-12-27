using System;

namespace DotNetLoxInterpreter.Exceptions;

public class LxRuntimeException : Exception
{
  public Token Token { get; }

  public LxRuntimeException(string message, Token token) : base(message)
  {
    Token = token;
  }
}
