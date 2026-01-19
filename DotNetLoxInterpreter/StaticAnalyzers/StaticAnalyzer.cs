namespace DotNetLoxInterpreter.StaticAnalyzers;

public class StaticAnalyzer : Stmt.IVisitorStmt<ValueType>, Expr.IVisitorExpr<ValueTask>
{
  private readonly IInterpreter _interpreter;
  private readonly Stack<Dictionary<Token, VariableSymanticMeta>> _scopes;
  private SymanticEnvironmentFlags _symanticEnvFlags = SymanticEnvironmentFlags.None;

  public StaticAnalyzer(IInterpreter interpreter)
  {
    _scopes = new();
    _interpreter = interpreter;
  }

  #region Statment Visit

  public ValueType Visit(Stmt.Function stmt)
  {
    Declare(stmt.Name);
    Define(stmt.Name);

    ResolveFunction(stmt, SymanticEnvironmentFlags.Function);

    return default!;
  }

  public ValueType Visit(Stmt.Block stmt)
  {
    BeginScope();
    Resolve(stmt.Statements);
    EndScope();

    return default!;
  }

  public ValueType Visit(Stmt.If stmt)
  {
    Resolve(stmt.Condition);
    Resolve(stmt.ThenBranch);
    if (stmt.ElseBranch is not null) Resolve(stmt.ElseBranch);

    return default!;
  }

  public ValueType Visit(Stmt.While stmt)
  {
    var enclosingSurroundings = _symanticEnvFlags;
    _symanticEnvFlags |= SymanticEnvironmentFlags.Loop;
    
    Resolve(stmt.Condition);
    Resolve(stmt.WhileBody);

    _symanticEnvFlags = enclosingSurroundings;

    return default!;
  }

  public ValueType Visit(Stmt.Expression stmt)
  {
    Resolve(stmt.Expr);

    return default!;
  }

  public ValueType Visit(Stmt.Print stmt)
  {
    Resolve(stmt.Value);

    return default!;
  }

  public ValueType Visit(Stmt.Break stmt)
  {
    if ((_symanticEnvFlags & SymanticEnvironmentFlags.Loop) != SymanticEnvironmentFlags.Loop)
    {
      DotnetLox.ReportError(stmt.Keyword, "Statment 'break' is not allowed outside of a loop's body.");
    }

    return default!;
  }

  public ValueType Visit(Stmt.Return stmt)
  {
    if ((_symanticEnvFlags & SymanticEnvironmentFlags.Function) != SymanticEnvironmentFlags.Function)
    {
      DotnetLox.ReportError(stmt.Keyword, "Can't return from top-level code.");
    }

    if (stmt.Value is not null) Resolve(stmt.Value);

    return default!;
  }

  public ValueType Visit(Stmt.Var stmt)
  {
    Declare(stmt.Name);

    if (stmt.Initializer is not null)
    {
      Resolve(stmt.Initializer);
    }

    Define(stmt.Name);

    return default!;
  }

  #endregion

  #region Expression Visit

  public ValueTask Visit(Expr.Ternary expr)
  {
    Resolve(expr.Left);
    Resolve(expr.Mid);
    Resolve(expr.Right);

    return default!;
  }

  public ValueTask Visit(Expr.Binary expr)
  {
    Resolve(expr.Left);
    Resolve(expr.Right);

    return default!;
  }

  public ValueTask Visit(Expr.Logical expr)
  {
    Resolve(expr.Left);
    Resolve(expr.Right);

    return default!;
  }

  public ValueTask Visit(Expr.Grouping expr)
  {
    Resolve(expr.Expr);

    return default!;
  }

  public ValueTask Visit(Expr.Literal expr)
  {
    return default!;
  }

  public ValueTask Visit(Expr.Unary expr)
  {
    Resolve(expr.Right);

    return default!;
  }

  public ValueTask Visit(Expr.Call expr)
  {
    Resolve(expr.Callee);

    foreach (var argument in expr.Arguments)
    {
      Resolve(argument);
    }

    return default!;
  }

  public ValueTask Visit(Expr.Lambda expr)
  {
    ResolveFunction(new Stmt.Function(expr.Name, expr.Parameters, expr.Body), SymanticEnvironmentFlags.Function);

    return default!;
  }

  public ValueTask Visit(Expr.Variable expr)
  {
    if (_scopes.Count != 0 && _scopes.Peek().TryGetValue(expr.Name, out var variableSymanticMeta) && variableSymanticMeta.IsDefined == false)
    {
      DotnetLox.ReportError(expr.Name, "Can't read local variable in its own initializer.");
    }

    MarkAsUsed(expr);
    ResolveLocal(expr, expr.Name);

    return default!;
  }

  public ValueTask Visit(Expr.Assign expr)
  {
    Resolve(expr.Value);
    ResolveLocal(expr, expr.Name);

    return default!;
  }

  #endregion

  public void Analyze(List<Stmt> stmts) => Resolve(stmts);

  private void Resolve(List<Stmt> stmts)
  {
    foreach (var stmt in stmts)
    {
      Resolve(stmt);
    }
  }

  private void MarkAsUsed(Expr.Variable expr)
  {
    var name = expr.Name;

    foreach (var scope in _scopes)
    {
      if (scope.ContainsKey(name))
      {
        scope[name].IsUsed = true;
        break;
      }
    }
  }

  private void Resolve(Stmt stmt)
  {
    stmt.Accept(this);
  }

  private void Resolve(Expr expr)
  {
    expr.Accept(this);
  }

  private void ResolveLocal(Expr expr, Token name)
  {
    int i = 0;

    foreach (var scope in _scopes)
    {
      if (scope.ContainsKey(name))
      {
        _interpreter.Resolve(expr, i);
        break;
      }

      i++;
    }
  }

  private void ResolveFunction(Stmt.Function stmt, SymanticEnvironmentFlags metaFlag)
  {
    var enclosingMeta = _symanticEnvFlags;
    // Invert loop flag to toggle it off once we enter the function body to avoid some edge cases
    // E.g: we are in the loop and we're creating functions inside (flag is 1),
    // but break in the body of the function makes no sense. (Turn of loop flag to avoid the confusion)
    _symanticEnvFlags = (_symanticEnvFlags | metaFlag) & ~SymanticEnvironmentFlags.Loop;

    BeginScope();

    foreach (var param in stmt.Parameters)
    {
      Declare(param);
      Define(param);
    }

    Resolve(stmt.Body);

    EndScope();

    _symanticEnvFlags = enclosingMeta;
  }

  private void Declare(Token name)
  {
    if (_scopes.Count == 0) return;

    var currentScope = _scopes.Peek();

    if (currentScope.ContainsKey(name))
    {
      DotnetLox.ReportError(name, $"The variable '{name.Lexeme}' is already defined in the scope.");
    }

    _scopes.Peek().Add(name, new VariableSymanticMeta());
  }

  private void Define(Token name)
  {
    if (_scopes.Count == 0) return;

    _scopes.Peek()[name].IsDefined = true;
  }

  private void BeginScope()
  {
    _scopes.Push(new());
  }

  private void EndScope()
  {
    var releasedScope = _scopes.Pop();

    foreach (var (name, meta) in releasedScope)
    {
      if (meta.IsUsed) continue;

      DotnetLox.ReportError(name, $"Variable '{name.Lexeme}' is declared, but unused.");
    }
  }
}
