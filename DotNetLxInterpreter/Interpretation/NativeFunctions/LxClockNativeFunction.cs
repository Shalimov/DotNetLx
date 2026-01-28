using DotNetLxInterpreter.Interpretation.LangAbstractions;

namespace DotNetLxInterpreter.Interpretation.NativeFunctions;

public class LxClockNativeFunction : ILxCallable
{
  public int Arity => 0;

  public object? Call(IInterpreter interpreter, IEnumerable<object?> arguments)
  {
    return DateTimeOffset.Now.ToUnixTimeSeconds();
  }

  public override string ToString() => "<native fn>";
}
