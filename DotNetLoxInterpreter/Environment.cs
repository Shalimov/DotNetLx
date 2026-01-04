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
}
