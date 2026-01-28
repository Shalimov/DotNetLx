namespace DotNetLxInterpreter.Interpretation.LangAbstractions;

public interface ILxCallable
{
  int Arity { get; }
  object? Call(IInterpreter interpreter, IEnumerable<object?> arguments);
}
