namespace DotNetLoxInterpreter;

public class InterpreterRepl : Interpreter, IInterpreter
{
  public override ValueType Visit(Stmt.Expression expr)
  {
    return Visit(new Stmt.Print(expr.Expr));
  }
}
