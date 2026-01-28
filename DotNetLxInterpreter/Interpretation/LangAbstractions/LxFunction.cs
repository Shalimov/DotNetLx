using DotNetLxInterpreter.FrontEnd;

namespace DotNetLxInterpreter.Interpretation.LangAbstractions;

public class LxFunction : ILxCallable
{
  private readonly Stmt.Function _function;
  private readonly Environment _clousre;
  public LxFunctionMeta Meta { get; }

  public LxFunction(Stmt.Function function, Environment clousre, LxFunctionMeta meta)
  {
    _function = function;
    _clousre = clousre;
    Meta = meta;
  }

  public int Arity => _function.Parameters.Count;

  public LxFunction Bind(LxInstance instance)
  {
    var enclosing = new Environment(_clousre);
    enclosing.Define("this", instance);

    return new LxFunction(_function, enclosing, Meta);
  }

  public object? Call(IInterpreter interpreter, IEnumerable<object?> arguments)
  {
    var environment = new Environment(_clousre);

    for (int i = 0, length = _function.Parameters.Count; i < length; i += 1)
    {
      environment.Define(_function.Parameters[i].Lexeme, arguments.ElementAt(i));
    }

    var executionResult = interpreter.ExecuteBlock(_function.Body, environment);

    if (Meta.IsInitializer)
    {
      return _clousre.GetAt(0, 0, new Token(TokenType.THIS, "this", null, _function.Name.Line, _function.Name.Column));
    }

    return executionResult.Payload;
  }

  public override string ToString() => $"<fn {_function.Name.Lexeme}>";
}
