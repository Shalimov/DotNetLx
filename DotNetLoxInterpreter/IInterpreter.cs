namespace DotNetLoxInterpreter;

public interface IInterpreter
{
  void Interpret(List<Stmt> stmts);

  ExecutionResult ExecuteBlock(List<Stmt> stmts, Environment environment); 
}
