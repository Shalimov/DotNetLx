using DotNetLoxInterpreter.FrontEnd;
using DotNetLoxInterpreter.Exceptions;

namespace DotNetLoxInterpreter.Interpretation;

public class Environment
{
  private readonly EnvironmentValueKeeper _envKeeper = new();

  public Environment? Enclosing { get; set; }

  public Environment()
  {
    Enclosing = null;
  }

  public Environment(Environment enclosing)
  {
    Enclosing = enclosing;
  }

  public void Define(string key)
  {
    _envKeeper.Declare(key);
  }

  public void Define(string key, object? value)
  {
    _envKeeper.Declare(key);
    _envKeeper[key].Value = value;
  }

  public void Assign(Token name, object? value)
  {
    if (_envKeeper.TryGetValueByKey(name.Lexeme, out var container) && container is not null)
    {
      container.Value = value;

      return;
    }

    if (Enclosing is not null)
    {
      Enclosing.Assign(name, value);

      return;
    }

    throw new LxRuntimeException($"Undefined variable '{name.Lexeme}'.", name);
  }

  public void AssignAt(int distance, int index, object? value)
  {
    var targetEnv = Ancestor(distance)._envKeeper;
    targetEnv[index].Value = value;
  }

  public object? Get(Token name)
  {
    if (_envKeeper.TryGetValueByKey(name.Lexeme, out var valueContainer) && valueContainer is not null)
    {
      if (!valueContainer.IsInitialized)
      {
        throw new LxRuntimeException($"Variable with name '{name.Lexeme}' is not initialized.", name);
      }

      return valueContainer.Value;
    }

    if (Enclosing is not null)
    {
      return Enclosing.Get(name);
    }

    throw new LxRuntimeException($"Variable with name '{name.Lexeme}' is not defined.", name);
  }

  // This code is executed presumably after static analisis
  // And all the variables are already marked and counted
  // So if it is called then "variable" exist (Main Assumption)
  // Thus some extra checks are removed
  public object? GetAt(int distance, int index, Token name)
  {
    var targetEnv = Ancestor(distance)._envKeeper;
    var valueContainer = targetEnv[index]!;

    if (!valueContainer.IsInitialized)
    {
      throw new LxRuntimeException($"Variable with name '{name.Lexeme}' is not initialized.", name);
    }

    return valueContainer.Value;
  }

  private Environment Ancestor(int distance)
  {
    Environment ancestorEnv = this;

    for (var i = 0; i < distance; i += 1)
    {
      ancestorEnv = ancestorEnv.Enclosing!;
    }

    return ancestorEnv;
  }
}
