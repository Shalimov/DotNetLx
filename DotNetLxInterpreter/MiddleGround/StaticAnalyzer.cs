using DotNetLxInterpreter.FrontEnd;
using DotNetLxInterpreter.Interpretation;

namespace DotNetLxInterpreter.MiddleGround;

public class StaticAnalyzer : Stmt.IVisitorStmt<ValueType>, Expr.IVisitorExpr<ValueType>
{
  private class ScopeData()
  {
    public int LastUniqIndex { get; set; } = 0;
    public Dictionary<Token, VariableSymanticMeta> Variables { get; } = new();
  }

  private readonly IInterpreter _interpreter;
  private readonly Stack<ScopeData> _scopes;
  private SymanticEnvironmentFlags _symanticEnvFlags = SymanticEnvironmentFlags.None;

  public StaticAnalyzer(IInterpreter interpreter)
  {
    _scopes = new();
    _interpreter = interpreter;
  }

  #region Statment Visit

  public ValueType Visit(Stmt.Class clsStmt)
  {
    Declare(clsStmt.Name);
    Define(clsStmt.Name);

    var enclosingSurroundings = _symanticEnvFlags;
    _symanticEnvFlags |= SymanticEnvironmentFlags.Class;

    if (clsStmt.SuperClass is not null)
    {
      if (clsStmt.Name.Equals(clsStmt.SuperClass.Name))
      {
        DotNetLx.ReportError(clsStmt.SuperClass.Name, "A class cannot inherit from itself.");
      }

      _symanticEnvFlags |= SymanticEnvironmentFlags.SubClass;

      Resolve(clsStmt.SuperClass);

      // 'super' should reside in its own scope to be higly accessable from sub-class methods
      // 'super' should be fixed to the context (who it points out to "situation")
      // and should not change context of call based on the level in inheritance chain
      BeginScope();

      var superToken = new Token(TokenType.SUPER, "super", null, clsStmt.Name.Line, clsStmt.Name.Column);
      DeclareReserved(superToken);
    }

    BeginScope();

    var thisToken = new Token(TokenType.THIS, "this", null, clsStmt.Name.Line, clsStmt.Name.Column);
    DeclareReserved(thisToken);

    var methodNameSet = new HashSet<string>();

    foreach (var method in clsStmt.Methods)
    {
      if (method.Modifier.HasFlag(FnModifier.Property) && method.Name.Lexeme.Equals("init"))
      {
        DotNetLx.ReportError(method.Name, $"Class property getter can not be named 'init'.");
      }

      if (methodNameSet.Contains(method.Name.Lexeme) && (method.Modifier.HasFlag(FnModifier.Property) || method.Modifier == FnModifier.None))
      {
        var kind = method.Modifier.HasFlag(FnModifier.Property) ? "property getter" : "method";

        DotNetLx.ReportError(method.Name, $"Class {kind} with name '{method.Name.Lexeme}' is the duplicate.");
      }
      else
      {
        methodNameSet.Add(method.Name.Lexeme);
      }
    }

    foreach (var method in clsStmt.Methods)
    {
      var declarationType = method.Modifier == FnModifier.None && method.Name.Lexeme.Equals("init") ?
        SymanticEnvironmentFlags.Initializer :
        SymanticEnvironmentFlags.Method;

      ResolveFunction(method, declarationType);
    }

    EndScope();

    if (clsStmt.SuperClass is not null)
    {
      EndScope();
    }

    _symanticEnvFlags = enclosingSurroundings;

    return default!;
  }

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
    if ((_symanticEnvFlags & SymanticEnvironmentFlags.Loop) == SymanticEnvironmentFlags.None)
    {
      DotNetLx.ReportError(stmt.Keyword, "Statment 'break' is not allowed outside of a loop's body.");
    }

    return default!;
  }

  public ValueType Visit(Stmt.Return stmt)
  {
    if ((_symanticEnvFlags & SymanticEnvironmentFlags.Callable) == SymanticEnvironmentFlags.None)
    {
      DotNetLx.ReportError(stmt.Keyword, "Can't return from top-level code.");
    }
    else if (stmt.Value is not null && (_symanticEnvFlags & SymanticEnvironmentFlags.Initializer) != SymanticEnvironmentFlags.None)
    {
      DotNetLx.ReportError(stmt.Keyword, "Can't return a value from an initializer.");
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

  public ValueType Visit(Expr.Ternary expr)
  {
    Resolve(expr.Left);
    Resolve(expr.Mid);
    Resolve(expr.Right);

    return default!;
  }

  public ValueType Visit(Expr.Binary expr)
  {
    Resolve(expr.Left);
    Resolve(expr.Right);

    return default!;
  }

  public ValueType Visit(Expr.Logical expr)
  {
    Resolve(expr.Left);
    Resolve(expr.Right);

    return default!;
  }

  public ValueType Visit(Expr.Grouping expr)
  {
    Resolve(expr.Expr);

    return default!;
  }

  public ValueType Visit(Expr.Literal expr)
  {
    return default!;
  }

  public ValueType Visit(Expr.Unary expr)
  {
    Resolve(expr.Right);

    return default!;
  }

  public ValueType Visit(Expr.Call expr)
  {
    Resolve(expr.Callee);

    foreach (var argument in expr.Arguments)
    {
      Resolve(argument);
    }

    return default!;
  }

  public ValueType Visit(Expr.Set expr)
  {
    Resolve(expr.Value);
    Resolve(expr.Target);

    return default!;
  }

  public ValueType Visit(Expr.Get expr)
  {
    Resolve(expr.Target);

    return default!;
  }

  public ValueType Visit(Expr.Super expr)
  {
    if ((_symanticEnvFlags & SymanticEnvironmentFlags.SubClass) == SymanticEnvironmentFlags.None)
    {
      DotNetLx.ReportError(expr.Keyword, "A usage of 'super' is forbidden outside of sub-classes.");
    }

    ResolveLocal(expr, expr.Keyword);

    return default!;
  }

  public ValueType Visit(Expr.This expr)
  {
    if ((_symanticEnvFlags & SymanticEnvironmentFlags.Class) == SymanticEnvironmentFlags.None)
    {
      DotNetLx.ReportError(expr.Keyword, "Expect 'this' only inside class methods.");

      return default!;
    }

    ResolveLocal(expr, expr.Keyword);

    return default!;
  }

  public ValueType Visit(Expr.Lambda expr)
  {
    ResolveFunction(new Stmt.Function(FnModifier.None, expr.Name, expr.Parameters, expr.Body), SymanticEnvironmentFlags.Function);

    return default!;
  }

  public ValueType Visit(Expr.Variable expr)
  {
    if (_scopes.Count == 0) return default!;

    var currentScope = _scopes.Peek();

    if (currentScope.Variables.TryGetValue(expr.Name, out var variableSymanticMeta) && variableSymanticMeta.IsDefined == false)
    {
      DotNetLx.ReportError(expr.Name, "Can't read local variable in its own initializer.");
    }

    MarkAsUsed(expr.Name);
    ResolveLocal(expr, expr.Name);

    return default!;
  }

  public ValueType Visit(Expr.Assign expr)
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
      if (scope.Variables.TryGetValue(name, out var variableSymanticMeta))
      {
        _interpreter.Resolve(expr, i, variableSymanticMeta.ScopeIndex);
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

    if (currentScope.Variables.ContainsKey(name))
    {
      DotNetLx.ReportError(name, $"The variable '{name.Lexeme}' is already defined in the scope.");
    }

    var nextIndex = currentScope.LastUniqIndex;

    currentScope.Variables.Add(name, new VariableSymanticMeta { ScopeIndex = nextIndex });
    currentScope.LastUniqIndex = nextIndex + 1;
  }

  private void Define(Token name)
  {
    if (_scopes.Count == 0) return;

    var currentScope = _scopes.Peek();
    currentScope.Variables[name].IsDefined = true;
  }

  private void MarkAsUsed(Token identifier)
  {
    foreach (var scope in _scopes)
    {
      if (scope.Variables.TryGetValue(identifier, out var variableSymanticMeta))
      {
        variableSymanticMeta.IsUsed = true;
        break;
      }
    }
  }

  /// <summary>
  /// Reserved words like "this", "super" are alwayes defined in appropriate contexts and can be always marked as "used".
  /// </summary>
  /// <param name="identifier"></param>
  private void DeclareReserved(Token identifier)
  {
    Declare(identifier);
    Define(identifier);
    MarkAsUsed(identifier);
  }

  private void BeginScope()
  {
    _scopes.Push(new ScopeData());
  }

  private void EndScope()
  {
    var releasedScope = _scopes.Pop();

    foreach (var (name, meta) in releasedScope.Variables)
    {
      if (meta.IsUsed) continue;

      DotNetLx.ReportError(name, $"Variable '{name.Lexeme}' is declared, but unused.");
    }
  }
}
