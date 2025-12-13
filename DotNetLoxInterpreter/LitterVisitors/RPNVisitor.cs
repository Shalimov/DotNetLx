using System;

namespace DotNetLoxInterpreter.LitterVisitors;

public class RPNVisitor : Expr.IVisitor<string>
{
  public string Visit(Expr.Binary expr)
  {
    return $"{expr.Left.Accept(this)} {expr.Right.Accept(this)} {expr.Op.Lexeme}";
  }

  public string Visit(Expr.Grouping expr)
  {
    return expr.Expr.Accept(this);
  }

  public string Visit(Expr.Literal expr)
  {
    return expr.Value.ToString() ?? "nil";
  }

  public string Visit(Expr.Unary expr)
  {
    return $"{expr.Right.Accept(this)} {expr.Op.Lexeme}";
  }

  public string Visit(Expr.Ternary expr)
  {
    throw new NotImplementedException("RPN Doesn't support ternary operators.");
  }

  // public static int Main(params string[] argv)
  // {
  //   var expr = new Expr.Binary(
  //     new Expr.Grouping(
  //       new Expr.Binary(
  //       new Expr.Literal(1),
  //       new Token(TokenType.PLUS, "+", null, 0, 0),
  //       new Expr.Literal(2)
  //       )
  //     ),
  //     new Token(TokenType.STAR, "*", null, 0, 0),
  //     new Expr.Grouping(
  //       new Expr.Binary(
  //       new Expr.Literal(4),
  //       new Token(TokenType.MINUS, "-", null, 0, 0),
  //       new Expr.Literal(3)
  //       )
  //     )
  //   );

  //   Console.WriteLine(new RPNVisitor().Visit(expr));

  //   return 0;
  // }
}
