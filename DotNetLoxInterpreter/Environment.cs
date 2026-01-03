using System;
using DotNetLoxInterpreter.Exceptions;

namespace DotNetLoxInterpreter;

public class Environment
{
  private readonly Dictionary<string, object?> _env = new ();

  public Environment? Enclosing { get; set; }

  public Environment()
  {
    Enclosing = null;
  }

  public Environment(Environment enclosing)
  {
    Enclosing = enclosing;
  }

  public void Define(string key, object? value)
  {
    _env.Add(key, value);
  }

  public void Assign(Token name, object? value)
  {
    if (_env.ContainsKey(name.Lexeme))
    {
      _env[name.Lexeme] = value;

      return;
    }

    if (Enclosing is not null)
    {
      Enclosing.Assign(name, value);

      return;
    }

    throw new LxRuntimeException($"Undefined variable '{name.Lexeme}'.", name);
  }

  public object? Get(Token name)
  {
    if (_env.TryGetValue(name.Lexeme, out object? value))
    {
      return value;
    }

    if (Enclosing is not null)
    {
      return Enclosing.Get(name);
    }

    throw new LxRuntimeException($"Variable with name {name.Lexeme} is not defined.", name);
  }
}
