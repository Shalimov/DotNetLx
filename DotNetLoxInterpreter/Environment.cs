using DotNetLoxInterpreter.Exceptions;

namespace DotNetLoxInterpreter;

public class Environment
{
  private class ValueContainer
  {
    private object? _value;

    public bool IsInitialized { get; private set; }
    public object? Value
    {
      get => _value;
      set
      {
        IsInitialized = true;
        _value = value;
      }
    }
  }


  private readonly Dictionary<string, ValueContainer> _env = new();

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
    _env.Add(key, new ValueContainer());
  }

  public void Define(string key, object? value)
  {
    _env.Add(key, new ValueContainer() { Value = value });
  }

  public void Assign(Token name, object? value)
  {
    if (_env.ContainsKey(name.Lexeme))
    {
      _env[name.Lexeme].Value = value;

      return;
    }

    if (Enclosing is not null)
    {
      Enclosing.Assign(name, value);

      return;
    }

    throw new LxRuntimeException($"Undefined variable '{name.Lexeme}'.", name);
  }

  public void AssignAt(int distance, Token name, object? value)
  {
    var targetEnv = Ancestor(distance)._env;
   targetEnv[name.Lexeme].Value = value; 
  }

  public object? Get(Token name)
  {
    if (_env.TryGetValue(name.Lexeme, out var valueContainer))
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
  public object? GetAt(int distance, Token name)
  {
    var targetEnv = Ancestor(distance)._env;
    var valueContainer = targetEnv[name.Lexeme]!;

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
