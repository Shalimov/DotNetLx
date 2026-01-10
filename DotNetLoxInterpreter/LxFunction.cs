
namespace DotNetLoxInterpreter;

public class LxFunction : ILxCallable
{
  private readonly Stmt.Function _function;

  public LxFunction(Stmt.Function function)
  {
    _function = function;
  }

  public int Arity => _function.Parameters.Count;

  public object? Call(IInterpreter interpreter, IEnumerable<object?> arguments, Environment invocationEnv)
  {
    var environment = new Environment(invocationEnv);

    for (int i = 0, length = _function.Parameters.Count; i < length; i += 1)
    {
      environment.Define(_function.Parameters[i].Lexeme, arguments.ElementAt(i));
    }

    _ = interpreter.ExecuteBlock(_function.Body, environment);

    return null;
  }

  public override string ToString() => $"<fn {_function.Name.Lexeme}>";
}
