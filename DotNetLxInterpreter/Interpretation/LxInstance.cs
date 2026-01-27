using DotNetLxInterpreter.FrontEnd;
using DotNetLxInterpreter.Exceptions;

namespace DotNetLxInterpreter.Interpretation;

public class LxInstance
{
  private readonly LxClass? _baseClass;
  private readonly Dictionary<string, object?> _fields = new ();

  public LxInstance(LxClass? baseClass)
  {
    _baseClass = baseClass;
  }

  public object? this[Token name]
  {
    get
    {
      if (_fields.ContainsKey(name.Lexeme))
      {
        return _fields[name.Lexeme];
      }

      var method = _baseClass?.FindMethod(name.Lexeme);

      if (method is not null)
      {
        return method.Bind(this);
      }

      throw new LxRuntimeException($"Undefined property '{name.Lexeme}'.", name);
    }

    set
    {
      _fields[name.Lexeme] = value;
    }
  }

  public override string ToString() => $"{_baseClass?.Name ?? "Metaclass"} Instance";
}
