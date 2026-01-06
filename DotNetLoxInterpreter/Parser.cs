using DotNetLoxInterpreter.Exceptions;

namespace DotNetLoxInterpreter;

public class Parser
{
  private readonly Token[] _tokens;
  private int _current = 0;

  public Parser(Token[] tokens)
  {
    _tokens = tokens;
  }

  public List<Stmt> Parse()
  {
    var statements = new List<Stmt>();

    while (!IsAtEnd())
    {
      // NOTE: Declaration might produce null statements that can be reject here
      var decl = Declaration();

      if (decl is null) continue;

      statements.Add(decl);
    }

    return statements;
  }

  #region Descent Parsing of Statements

  private Stmt? Declaration()
  {
    try
    {
      if (Match(TokenType.VAR)) return VarDecl();

      return Statement();
    }
    catch (LxParseException)
    {
      Synchronize();
      // DotnetLox.ReportError(error.Token, error.Message);
      return null;
    }
  }

  private Stmt VarDecl()
  {
    var name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

    Expr initializer = null!;

    if (Match(TokenType.EQUAL))
    {
      initializer = Expression();
    }

    Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
    
    return new Stmt.Var(name, initializer);
  }

  private Stmt Statement()
  {
    if (Match(TokenType.IF)) return IfStmt();
    if (Match(TokenType.PRINT)) return PrintStmt();
    if (Match(TokenType.LEFT_BRACE)) return BlockStmt();

    return ExprStmt();
  }

  public Stmt IfStmt()
  {
    Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");

    var condition = Expression();

    Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

    var thenStatment = Statement();

    Stmt? elseStatment = null;

    if (Match(TokenType.ELSE))
    {
      elseStatment = Statement();
    }

    return new Stmt.If(condition, thenStatment, elseStatment);
  }

  public Stmt BlockStmt() => new Stmt.Block(Block());

  public List<Stmt> Block()
  {
    var stmts = new List<Stmt>();

    while (!(Check(TokenType.RIGHT_BRACE) || IsAtEnd()))
    {
      var stmt = Declaration();

      if (stmt is null) continue;

      stmts.Add(stmt);
    }

    Consume(TokenType.RIGHT_BRACE, "Block should be closed.");

    return stmts;
  }

  private Stmt ExprStmt()
  {
    var expression = Expression();
    Consume(TokenType.SEMICOLON, "Expect ';' after expression.");

    return new Stmt.Expression(expression);
  }

  private Stmt PrintStmt()
  {
    var value = Expression();
    Consume(TokenType.SEMICOLON, "Expect ';' after value.");

    return new Stmt.Print(value);
  }

  #endregion

  #region Descent Parsing Of Expressions

  private Expr Expression()
  {
    return Assignment();
  }

  private Expr Assignment()
  {
    var expr = Commaseq();

    if (Match(TokenType.EQUAL))
    {
      var equals = Previous();
      var value = Assignment();

      if (expr is Expr.Variable variable)
      {
        return new Expr.Assign(variable.Name, value);
      }

      Error(equals, "Invalid assignment target.");
    }

    return expr;
  }

  private Expr Commaseq()
  {
    var leadingExpr = LogicOr();

    while (Match(TokenType.COMMA))
    {
      var token = Previous();
      var trailingExpr = LogicOr();

      leadingExpr = new Expr.Binary(leadingExpr, token, trailingExpr);
    }

    return leadingExpr;
  }

  private Expr LogicOr()
  {
     var leadingExpr = LogicAnd();

    while (Match(TokenType.OR))
    {
      var token = Previous();
      var trailingExpr = LogicAnd();

      leadingExpr = new Expr.Logical(leadingExpr, token, trailingExpr);
    }

    return leadingExpr;
  }

  private Expr LogicAnd()
  {
    var leadingExpr = Ternary();

    while (Match(TokenType.AND))
    {
      var token = Previous();
      var trailingExpr = Ternary();

      leadingExpr = new Expr.Logical(leadingExpr, token, trailingExpr);
    }

    return leadingExpr;
  }

  private Expr Ternary()
  {
    var leadingExpr = Equality();

    if (Match(TokenType.QUESTION_MARK))
    {
      var questionToken = Previous();
      var midExpr = Ternary();
      var colonToken = Consume(TokenType.COLON, "Expected a complete ternary operator, but ':' is not find.");
      var tailingExpr = Ternary();

      leadingExpr = new Expr.Ternary(leadingExpr, questionToken, midExpr, colonToken, tailingExpr);
    }

    return leadingExpr;
  }

  private Expr Equality()
  {
    var leadingExpr = Comparison();

    while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
    {
      var token = Previous();
      var trailingExpr = Comparison();

      leadingExpr = new Expr.Binary(leadingExpr, token, trailingExpr);
    }

    return leadingExpr;
  }

  private Expr Comparison()
  {
    var leadingExpr = Term();

    while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
    {
      var token = Previous();
      var trailingExpr = Term();

      leadingExpr = new Expr.Binary(leadingExpr, token, trailingExpr);
    }

    return leadingExpr;
  }

  private Expr Term()
  {
    var leadingExpr = Factor();

    while (Match(TokenType.MINUS, TokenType.PLUS))
    {
      var token = Previous();
      var trailingExpr = Factor();

      leadingExpr = new Expr.Binary(leadingExpr, token, trailingExpr);
    }

    return leadingExpr;
  }

  private Expr Factor()
  {
    var leadingExpr = Unary();

    while (Match(TokenType.SLASH, TokenType.STAR))
    {
      var token = Previous();
      var trailingExpr = Unary();

      leadingExpr = new Expr.Binary(leadingExpr, token, trailingExpr);
    }

    return leadingExpr;
  }

  // Original grammar:
  // unary -> ( "!" | "-" ) unary | primary ;
  // is modified to catch cases with more binary operators like:
  // unary -> ( "!" | "-" | "+" | "*" | "/" ) unary | primary ;
  // but we report an error when preceding operator is not "!" or "-"
  private Expr Unary()
  {
    if (Match(TokenType.BANG, TokenType.MINUS))
    {
      var token = Previous();
      var trailingExpr = Unary();

      return new Expr.Unary(token, trailingExpr);
    }
    else if (Match(TokenType.PLUS, TokenType.SLASH, TokenType.STAR))
    {
      var token = Previous();
      // Go deeper without throwing error right away
      // to provide better error reporting and in addition skip unnecessary tokens
      _ = Unary();

      throw Error(token, $"Unary operator '{token.Lexeme}' is not supported.");
    }

    return Primary();
  }

  private Expr Primary()
  {
    if (Match(TokenType.FALSE)) return new Expr.Literal(false);
    if (Match(TokenType.TRUE)) return new Expr.Literal(true);
    if (Match(TokenType.NIL)) return new Expr.Literal(null!);

    if (Match(TokenType.NUMBER, TokenType.STRING))
    {
      var token = Previous();
      return new Expr.Literal(token.Literal!);
    }

    if (Match(TokenType.IDENTIFIER))
    {
      var name = Previous();
      return new Expr.Variable(name);
    }

    if (Match(TokenType.LEFT_PAREN))
    {
      var leadingExpr = Expression();
      Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");

      return new Expr.Grouping(leadingExpr);
    }

    throw Error(Peek(), "Expression is expected.");
  }

  #endregion

  #region Sync

  /*
    Synchronization is really important part of the parsing process
    When an error occures we try our best to get to the next valid expression to keep on parsing.
    It suppose to work because we unwind the stack of call frames to the parser's state
    we consider safe. It happens with throwing exception and catching it at the level of "safe" language construction.

    It helps to achieve a tradeoff where we avoid cascading errors (being inside incorrect state and keep on parsing)
    and check whether we have anything ahead that is worth to report.
  */
  private void Synchronize()
  {
    Advance();

    while (!IsAtEnd())
    {
      if (Previous().Type == TokenType.SEMICOLON) return;

      switch (Peek().Type)
      {
        case TokenType.CLASS:
        case TokenType.IF:
        case TokenType.FOR:
        case TokenType.WHILE:
        case TokenType.VAR:
        case TokenType.FUN:
        case TokenType.RETURN:
        case TokenType.PRINT:
          return;
      }

      Advance();
    }
  }

  #endregion

  #region Helpers

  private Token Consume(TokenType token, string rejectionReason)
  {
    if (Check(token)) return Advance();

    throw Error(Peek(), rejectionReason);
  }

  private bool Match(params TokenType[] tokenTypes)
  {
    if (tokenTypes.Any(Check))
    {
      Advance();

      return true;
    }

    return false;
  }

  private Token Peek() => _tokens[_current];

  private Token Previous() => _tokens[_current - 1];

  private Token Advance()
  {
    if (!IsAtEnd()) _current++;

    return Previous();
  }

  private LxParseException Error(Token stopToken, string message)
  {
    DotnetLox.ReportError(stopToken, message);

    return new LxParseException(message, stopToken);
  }

  private bool Check(TokenType tokenType)
  {
    if (IsAtEnd()) return false;

    return Peek().Type == tokenType;
  }

  private bool IsAtEnd() => Peek().Type == TokenType.EOF;

  #endregion
}
