namespace DotNetLoxInterpreter;

public interface IInterpreter
{
  void Interpret(List<Stmt> stmts);
}
