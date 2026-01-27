namespace DotNetLxInterpreter.Interpretation;

public class LxClass : LxInstance, ILxCallable
{
  private readonly string _name;
  private readonly Dictionary<string, LxFunction> _methods;
  
  public LxClass(string name, Dictionary<string, LxFunction> methods) : this(null!, name, methods)
  {
    
  }

  public LxClass(LxClass baseClass, string name, Dictionary<string, LxFunction> methods) : base(baseClass)
  {
    _name = name;
    _methods = methods;
  }

  public string Name => _name;

  public int Arity
  {
    get
    {
      var initializer = FindMethod("init");
      return initializer?.Arity ?? 0;
    }
  }

  public LxFunction? FindMethod(string name)
  {
    if (_methods.ContainsKey(name))
    {
      return _methods[name];
    }

    return null;
  }

  public object? Call(IInterpreter interpreter, IEnumerable<object?> arguments)
  {
    var instance = new LxInstance(this);
    var initializer = FindMethod("init");

    initializer?.Bind(instance).Call(interpreter, arguments);

    return instance;
  }

  public override string ToString() => _name;
}
