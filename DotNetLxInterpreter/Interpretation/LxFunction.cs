using DotNetLxInterpreter.FrontEnd;

namespace DotNetLxInterpreter.Interpretation;

public class LxFunction : ILxCallable
{
  private readonly Stmt.Function _function;
  private readonly Environment _clousre;
  private readonly bool _isInitializer;

  public LxFunction(Stmt.Function function, Environment clousre, bool isInitializer)
  {
    _function = function;
    _clousre = clousre;
    _isInitializer = isInitializer;
  }

  public int Arity => _function.Parameters.Count;

  public LxFunction Bind(LxInstance instance)
  {
    var enclosing = new Environment(_clousre);
    enclosing.Define("this", instance);

    return new LxFunction(_function, enclosing, _isInitializer);
  }

  public object? Call(IInterpreter interpreter, IEnumerable<object?> arguments)
  {
    var environment = new Environment(_clousre);

    for (int i = 0, length = _function.Parameters.Count; i < length; i += 1)
    {
      environment.Define(_function.Parameters[i].Lexeme, arguments.ElementAt(i));
    }

    var executionResult = interpreter.ExecuteBlock(_function.Body, environment);

    if (_isInitializer)
    {
      return _clousre.GetAt(0, 0, new Token(TokenType.THIS, "this", null, _function.Name.Line, _function.Name.Column));
    }

    return executionResult.Payload;
  }

  public override string ToString() => $"<fn {_function.Name.Lexeme}>";
}
