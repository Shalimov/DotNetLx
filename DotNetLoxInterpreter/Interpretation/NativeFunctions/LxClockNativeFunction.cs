namespace DotNetLoxInterpreter.Interpretation.NativeFunctions;

public class LxClockNativeFunction : ILxCallable
{
  public int Arity => 0;

  public object? Call(IInterpreter interpreter, IEnumerable<object?> arguments)
  {
    return DateTimeOffset.Now.ToUnixTimeSeconds();
  }

  public override string ToString() => "<native fn>";
}
