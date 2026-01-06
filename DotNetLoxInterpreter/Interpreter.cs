using System.Collections;
using DotNetLoxInterpreter.Exceptions;
using static DotNetLoxInterpreter.LxRuntimeAssertions;

namespace DotNetLoxInterpreter;

public class Interpreter : Expr.IVisitorExpr<object?>, Stmt.IVisitorStmt<ValueType>, IInterpreter
{
  private Environment _environment = new();

  #region Statement Visits

  public ValueType Visit(Stmt.If ifStmt)
  {
    var conditionValue = Evaluate(ifStmt.Condition);

    if (IsTruthy(conditionValue))
    {
      Execute(ifStmt.ThenBranch);
    }
    else if (ifStmt.ElseBranch is not null)
    {
      Execute(ifStmt.ElseBranch);
    }

    return default!;
  }

  public ValueType Visit(Stmt.While whileStmt)
  {
    while (IsTruthy(Evaluate(whileStmt.Condition)))
    {
      Execute(whileStmt.WhileBody);
    }

    return default!;
  }

  public ValueType Visit(Stmt.Block block)
  {
    ExecuteBlock(block.Statements, new Environment(_environment));

    return default!;
  }

  public virtual ValueType Visit(Stmt.Expression expr)
  {
    Evaluate(expr.Expr);

    return default!;
  }

  public ValueType Visit(Stmt.Print expr)
  {
    var evaluatedValue = Evaluate(expr.Value);
    Console.Out.WriteLine(Stringify(evaluatedValue));

    return default!;
  }

  public ValueType Visit(Stmt.Var expr)
  {
    _environment.Define(expr.Name.Lexeme);

    if (expr.Initializer is not null)
    {
      var value = Evaluate(expr.Initializer);

      _environment.Assign(expr.Name, value);
    }

    return default!;
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
        return leftValue!.ToString() + rightValue!.ToString();
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

  public object? Visit(Expr.Variable expr)
  {
    return _environment.Get(expr.Name);
  }

  public object? Visit(Expr.Assign expr)
  {
    var value = Evaluate(expr.Value);
    _environment.Assign(expr.Name, value);

    return value;
  }

  #endregion

  public void Interpret(List<Stmt> stmts)
  {
    try
    {
      foreach (var stmt in stmts)
      {
        Execute(stmt);
      }
    }
    catch (LxRuntimeException ex)
    {
      DotnetLox.RuntimeError(ex);
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

  private void ExecuteBlock(List<Stmt> stmts, Environment environment)
  {
    var previousEnv = _environment;

    try
    {
      _environment = environment;

      foreach (var stmt in stmts)
      {
        Execute(stmt);
      }
    }
    finally
    {
      _environment = previousEnv;
    }
  }

  private void Execute(Stmt stmt)
  {
    stmt.Accept(this);
  }
}
