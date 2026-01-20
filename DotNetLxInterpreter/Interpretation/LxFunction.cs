using DotNetLxInterpreter.FrontEnd;

namespace DotNetLxInterpreter.Interpretation;

public class LxFunction : ILxCallable
{
  private readonly Stmt.Function _function;
  private readonly Environment _clousre;

  public LxFunction(Stmt.Function function, Environment clousre)
  {
    _function = function;
    _clousre = clousre;
  }

  public int Arity => _function.Parameters.Count;

  public object? Call(IInterpreter interpreter, IEnumerable<object?> arguments)
  {
    var environment = new Environment(_clousre);

    for (int i = 0, length = _function.Parameters.Count; i < length; i += 1)
    {
      environment.Define(_function.Parameters[i].Lexeme, arguments.ElementAt(i));
    }

    var executionResult = interpreter.ExecuteBlock(_function.Body, environment);

    return executionResult.Payload;
  }

  public override string ToString() => $"<fn {_function.Name.Lexeme}>";
}
