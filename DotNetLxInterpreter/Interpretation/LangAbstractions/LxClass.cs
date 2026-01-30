namespace DotNetLxInterpreter.Interpretation.LangAbstractions;

public class LxClass : LxInstance, ILxCallable
{
  private readonly string _name;
  private readonly LxClass? _superclass;
  private readonly Dictionary<string, LxFunction> _methods;

  public static LxClass MetaClass(Dictionary<string, LxFunction> methods)
  {
    return new LxClass(null!, "Metaclass", null, methods);
  }

  public LxClass(LxClass metaclass, string name, LxClass? superclass, Dictionary<string, LxFunction> methods) : base(metaclass)
  {
    _name = name;
    _methods = methods;
    _superclass = superclass;
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

    return _superclass?.FindMethod(name);
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
