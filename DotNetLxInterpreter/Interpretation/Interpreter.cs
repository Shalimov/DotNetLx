using System.Collections;
using DotNetLxInterpreter.FrontEnd;
using DotNetLxInterpreter.Exceptions;
using DotNetLxInterpreter.Interpretation.NativeFunctions;
using DotNetLxInterpreter.Interpretation.LangAbstractions;
using static DotNetLxInterpreter.Interpretation.LxRuntimeAssertions;

namespace DotNetLxInterpreter.Interpretation;

public class Interpreter : Expr.IVisitorExpr<object?>, Stmt.IVisitorStmt<ExecutionResult>, IInterpreter
{
  private readonly Dictionary<Expr, (int Level, int ScopeIndex)?> _locals;
  private readonly Environment _globals;
  private Environment _environment;

  public Interpreter()
  {
    _locals = new();
    _globals = new();
    _environment = _globals;

    _globals.Define("clock", new LxClockNativeFunction());
  }

  #region Statement Visits

  public ExecutionResult Visit(Stmt.If ifStmt)
  {
    var conditionValue = Evaluate(ifStmt.Condition);

    if (IsTruthy(conditionValue))
    {
      return Execute(ifStmt.ThenBranch);
    }
    else if (ifStmt.ElseBranch is not null)
    {
      return Execute(ifStmt.ElseBranch);
    }

    return ExecutionResult.Normal;
  }

  public ExecutionResult Visit(Stmt.While whileStmt)
  {
    while (IsTruthy(Evaluate(whileStmt.Condition)))
    {
      var result = Execute(whileStmt.WhileBody);

      if (result == ExecutionResult.Break)
      {
        break;
      }
      else if (result == ExecutionResult.Return)
      {
        return result;
      }
    }

    return ExecutionResult.Normal;
  }
  public ExecutionResult Visit(Stmt.Block blockStmt) => ExecuteBlock(blockStmt.Statements, new Environment(_environment));

  public ExecutionResult Visit(Stmt.Break brkStmt) => ExecutionResult.Break;

  public ExecutionResult Visit(Stmt.Return retStmt)
  {
    if (retStmt.Value is null)
    {
      return ExecutionResult.Return;
    }

    var value = Evaluate(retStmt.Value);

    return ExecutionResult.ReturnWithPayload(value);
  }

  public virtual ExecutionResult Visit(Stmt.Expression exprStmt)
  {
    Evaluate(exprStmt.Expr);

    return ExecutionResult.Normal;
  }

  public ExecutionResult Visit(Stmt.Print printStmt)
  {
    var evaluatedValue = Evaluate(printStmt.Value);
    Console.Out.WriteLine(Stringify(evaluatedValue));

    return ExecutionResult.Normal;
  }

  public ExecutionResult Visit(Stmt.Class clsDecl)
  {
    object? superclass = null;

    if (clsDecl.SuperClass is not null)
    {
      superclass = Evaluate(clsDecl.SuperClass);

      if (superclass is not LxClass)
      {
        throw new LxRuntimeException("A superclass must be a class.", clsDecl.SuperClass.Name);
      }
    }

    _environment.Define(clsDecl.Name.Lexeme, null);

    var staticMethods = clsDecl.StaticMethods.ToDictionary(
      method => method.Name.Lexeme,
      method => new LxFunction(method, _environment, new LxFunctionMeta()));

    var methods = clsDecl.Methods.ToDictionary(
      method => method.Name.Lexeme,
      method => new LxFunction(method, _environment, new LxFunctionMeta { IsInitializer = method.Name.Lexeme.Equals("init") }));

    foreach (var getter in clsDecl.Properties)
    {
      methods.Add(getter.Name.Lexeme, new LxFunction(getter, _environment, new LxFunctionMeta() { IsProperty = true }));
    }

    var lxClass = new LxClass(
      LxClass.MetaClass(staticMethods),
      clsDecl.Name.Lexeme,
      superclass as LxClass, 
      methods);

    _environment.Assign(clsDecl.Name, lxClass);

    return ExecutionResult.Normal;
  }

  public ExecutionResult Visit(Stmt.Function funDecl)
  {
    _environment.Define(funDecl.Name.Lexeme, new LxFunction(funDecl, _environment, new LxFunctionMeta()));

    return ExecutionResult.Normal;
  }

  public ExecutionResult Visit(Stmt.Var varDecl)
  {
    _environment.Define(varDecl.Name.Lexeme);

    if (varDecl.Initializer is not null)
    {
      var value = Evaluate(varDecl.Initializer);

      _environment.Assign(varDecl.Name, value);
    }

    return ExecutionResult.Normal;
  }

  #endregion

  #region Expression Visits

  public object? Visit(Expr.Ternary expr)
  {
    var conditionValue = Evaluate(expr.Left);

    if (IsTruthy(conditionValue))
    {
      return Evaluate(expr.Mid);
    }
    else
    {
      return Evaluate(expr.Right);
    }
  }

  public object? Visit(Expr.Binary expr)
  {
    var leftValue = Evaluate(expr.Left);
    var rightValue = Evaluate(expr.Right);

    if (expr.Op.Type == TokenType.MINUS)
    {
      AssertNumberOperands(expr.Op, leftValue, rightValue);

      return (double)leftValue! - (double)rightValue!;
    }

    if (expr.Op.Type == TokenType.SLASH)
    {
      AssertNumberOperands(expr.Op, leftValue, rightValue);

      var rv = (double)rightValue!;

      AssertNonNullDivisor(expr.Op, rv);

      return (double)leftValue! / rv;
    }

    if (expr.Op.Type == TokenType.STAR)
    {
      AssertNumberOperands(expr.Op, leftValue, rightValue);

      return (double)leftValue! * (double)rightValue!;
    }

    if (expr.Op.Type == TokenType.PLUS)
    {
      if (leftValue is double lv && rightValue is double rv)
      {
        return lv + rv;
      }

      if (leftValue is string || rightValue is string)
      {
        var leftStr = leftValue ?? "nil";
        var rightStr = rightValue ?? "nil";

        return leftStr.ToString() + rightStr.ToString();
      }

      throw new LxRuntimeException("Operands should be numbers or convertable to strings", expr.Op);
    }

    if (expr.Op.Type == TokenType.LESS)
    {
      AssertSameTypeOperands(expr.Op, leftValue, rightValue);

      return Comparer.Default.Compare(leftValue, rightValue) < 0;
    }

    if (expr.Op.Type == TokenType.LESS_EQUAL)
    {
      AssertSameTypeOperands(expr.Op, leftValue, rightValue);

      return Comparer.Default.Compare(leftValue, rightValue) <= 0;
    }

    if (expr.Op.Type == TokenType.GREATER)
    {
      AssertSameTypeOperands(expr.Op, leftValue, rightValue);

      return Comparer.Default.Compare(leftValue, rightValue) > 0;
    }

    if (expr.Op.Type == TokenType.GREATER_EQUAL)
    {
      AssertSameTypeOperands(expr.Op, leftValue, rightValue);

      return Comparer.Default.Compare(leftValue, rightValue) >= 0;
    }

    if (expr.Op.Type == TokenType.BANG_EQUAL)
    {
      return !IsEqual(leftValue, rightValue);
    }

    if (expr.Op.Type == TokenType.EQUAL_EQUAL)
    {
      return IsEqual(leftValue, rightValue);
    }

    if (expr.Op.Type == TokenType.COMMA)
    {
      return rightValue;
    }

    return null;
  }

  public object? Visit(Expr.Logical expr)
  {
    var leftValue = Evaluate(expr.Left);

    if (expr.Op.Type == TokenType.OR)
    {
      if (IsTruthy(leftValue)) return leftValue;
    }
    else
    {
      if (!IsTruthy(leftValue)) return leftValue;
    }

    return Evaluate(expr.Right);
  }

  public object? Visit(Expr.Grouping expr)
  {
    return Evaluate(expr.Expr);
  }

  public object? Visit(Expr.Literal expr)
  {
    return expr.Value;
  }

  public object? Visit(Expr.Unary expr)
  {
    var value = Evaluate(expr.Right);

    if (expr.Op.Type == TokenType.MINUS)
    {
      AssertNumberOperand(expr.Op, value);

      return -(double)value!;
    }

    if (expr.Op.Type == TokenType.BANG)
    {
      return !IsTruthy(value);
    }

    return null;
  }

  public object? Visit(Expr.Set expr)
  {
    var target = Evaluate(expr.Target);

    if (target is not LxInstance lxInstance)
    {
      throw new LxRuntimeException("Only instances can have fields.", expr.Name);
    }

    var value = Evaluate(expr.Value);
    lxInstance[expr.Name] = value;

    return value;
  }

  public object? Visit(Expr.Get expr)
  {
    var target = Evaluate(expr.Target);

    if (target is LxInstance lxInstance)
    {
      var instanceValue = lxInstance[expr.Name];

      if (instanceValue is LxFunction instanceMethod && instanceMethod.Meta.IsProperty)
      {
        return instanceMethod.Call(this, []);
      }
      else
      {
        return instanceValue;
      }
    }

    throw new LxRuntimeException("Only instances can have fields.", expr.Name);
  }

  public object? Visit(Expr.Call expr)
  {
    var callee = Evaluate(expr.Callee);
    var arguments = expr.Arguments.Select(Evaluate);

    if (callee is ILxCallable fn)
    {
      if (arguments.Count() != fn.Arity)
      {
        throw new LxRuntimeException($"Expected {fn.Arity} arguments, but got {arguments.Count()}.", expr.TraceParen);
      }

      return fn.Call(this, arguments);
    }

    throw new LxRuntimeException("Only classes or functions are callable.", expr.TraceParen);
  }

  public object? Visit(Expr.This expr) => LookupVariable(expr.Keyword, expr);

  public object? Visit(Expr.Variable expr) => LookupVariable(expr.Name, expr);

  public object? Visit(Expr.Assign expr)
  {
    var value = Evaluate(expr.Value);
    if (_locals.TryGetValue(expr, out var location) && location.HasValue)
    {
      _environment.AssignAt(location.Value.Level, location.Value.ScopeIndex, value);
    }
    else
    {
      _globals.Assign(expr.Name, value);
    }

    return value;
  }

  public object? Visit(Expr.Lambda lambda) => new LxFunction(new Stmt.Function(lambda.Name, lambda.Parameters, lambda.Body), _environment, new LxFunctionMeta());

  #endregion

  public void Interpret(List<Stmt> stmts)
  {
    try
    {
      foreach (var stmt in stmts)
      {
        _ = Execute(stmt);
      }
    }
    catch (LxRuntimeException ex)
    {
      DotNetLx.RuntimeError(ex);
    }
  }

  public ExecutionResult ExecuteBlock(List<Stmt> stmts, Environment environment)
  {
    var previousEnv = _environment;

    try
    {
      _environment = environment;

      foreach (var stmt in stmts)
      {
        var result = Execute(stmt);

        if (result != ExecutionResult.Normal)
        {
          return result;
        }
      }
    }
    finally
    {
      _environment = previousEnv;
    }

    return ExecutionResult.Normal;
  }

  public void Resolve(Expr expr, int depth, int index)
  {
    _locals.TryAdd(expr, (Level: depth, ScopeIndex: index));
  }

  private object? LookupVariable(Token name, Expr expr)
  {
    if (_locals.TryGetValue(expr, out var location) && location.HasValue)
    {
      return _environment.GetAt(location.Value.Level, location.Value.ScopeIndex, name);
    }
    else
    {
      return _globals.Get(name);
    }
  }

  private string Stringify(object? result)
  {
    if (result is null) return "nil";

    if (result is double res)
    {
      var text = res.ToString();

      if (text!.EndsWith(".0"))
      {
        return text[..-2];
      }
    }

    return result.ToString()!;
  }

  private bool IsEqual(object? left, object? right)
  {
    if (left is null && right is null) return true;
    if (left is null) return false;

    return left.Equals(right);
  }

  private bool IsTruthy(object? value)
  {
    if (value is null) return false;
    if (value is bool result) return result;

    return true;
  }

  private object? Evaluate(Expr expr)
  {
    return expr.Accept(this);
  }

  private ExecutionResult Execute(Stmt stmt)
  {
    return stmt.Accept(this);
  }
}
