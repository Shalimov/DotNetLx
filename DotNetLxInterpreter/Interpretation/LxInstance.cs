using DotNetLxInterpreter.FrontEnd;
using DotNetLxInterpreter.Exceptions;

namespace DotNetLxInterpreter.Interpretation;

public class LxInstance(LxClass baseClass)
{
  private readonly Dictionary<string, object?> _fields = new ();

  public object? this[Token name]
  {
    get
    {
      if (_fields.ContainsKey(name.Lexeme))
      {
        return _fields[name.Lexeme];
      }

      throw new LxRuntimeException($"Undefined property '{name.Lexeme}'.", name);
    }

    set
    {
      _fields.Add(name.Lexeme, value);
    }
  }

  public override string ToString() => $"{baseClass.Name} Instance";
}
