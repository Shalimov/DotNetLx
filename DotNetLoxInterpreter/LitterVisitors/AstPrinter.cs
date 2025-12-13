using System;
using System.Text;

namespace DotNetLoxInterpreter.LitterVisitors;

public class AstPrinter : Expr.IVisitor<string>
{
  public string Print(Expr? expr)
  {
    if (expr is null)
    {
      return string.Empty;
    }
    
    return expr.Accept(this);
  }

  public string Visit(Expr.Binary expr)
  {
    return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
  }

  public string Visit(Expr.Grouping expr)
  {
    return Parenthesize("group", expr.Expr);
  }

  public string Visit(Expr.Literal expr)
  {
    return expr.Value?.ToString() ?? "nil";
  }

  public string Visit(Expr.Unary expr)
  {
    return Parenthesize(expr.Op.Lexeme, expr.Right);
  }

  public string Visit(Expr.Ternary expr)
  {
    return Parenthesize($"ternary {expr.Op1.Lexeme}{expr.Op2.Lexeme}", expr.Left, expr.Mid, expr.Right);
  }

  private string Parenthesize(string name, params Expr[] exprs)
  {
    StringBuilder stringBuilder = new StringBuilder();

    stringBuilder.AppendFormat("({0}", name);

    foreach (var expr in exprs)
    {
      stringBuilder.Append(' ');
      stringBuilder.Append(expr.Accept(this));
    }

    stringBuilder.Append(')');

    return stringBuilder.ToString();
  }
}