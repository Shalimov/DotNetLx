namespace DotNetLoxInterpreter;

public interface IInterpreter
{
  void Interpret(List<Stmt> stmts);

  void Resolve(Expr expr, int depth);

  ExecutionResult ExecuteBlock(List<Stmt> stmts, Environment environment); 
}
