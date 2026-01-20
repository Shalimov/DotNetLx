namespace DotNetLoxInterpreter.Interpretation;

public interface ILxCallable
{
  int Arity { get; }
  object? Call(IInterpreter interpreter, IEnumerable<object?> arguments);
}
