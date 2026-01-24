namespace DotNetLxInterpreter.Interpretation;

public class LxClass(string name, Dictionary<string, LxFunction> methods) : ILxCallable
{
  private readonly Dictionary<string, LxFunction> _methods = methods;
  public string Name => name;

  public int Arity => 0;

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
    
    return instance;
  }

  public override string ToString() => name;
}
