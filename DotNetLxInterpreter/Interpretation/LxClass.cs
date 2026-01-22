namespace DotNetLxInterpreter.Interpretation;

public class LxClass(string name) : ILxCallable
{
  public string Name => name;

  public int Arity => 0;

  public object? Call(IInterpreter interpreter, IEnumerable<object?> arguments)
  {
    var instance = new LxInstance(this);
    
    return instance;
  }

  public override string ToString() => name;
}
