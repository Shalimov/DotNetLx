using DotNetLoxInterpreter.FrontEnd;

namespace DotNetLoxInterpreter.Interpretation;

public class InterpreterRepl : Interpreter, IInterpreter
{
  public override ExecutionResult Visit(Stmt.Expression expr)
  {
    return Visit(new Stmt.Print(expr.Expr));
  }
}
