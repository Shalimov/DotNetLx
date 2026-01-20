using DotNetLoxInterpreter.FrontEnd;

namespace DotNetLoxInterpreter.Interpretation;

public interface IInterpreter
{
  void Interpret(List<Stmt> stmts);

  void Resolve(Expr expr, int depth, int index);

  ExecutionResult ExecuteBlock(List<Stmt> stmts, Environment environment); 
}
