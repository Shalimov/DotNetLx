/* IMPORTANT: Generated Code Do Not Amend */
namespace DotNetLxInterpreter.FrontEnd
{
    public abstract class Expr
    {
        public interface IVisitorExpr<out TR>
        {
            public TR Visit(Ternary expr);
            public TR Visit(Binary expr);
            public TR Visit(Logical expr);
            public TR Visit(Grouping expr);
            public TR Visit(Literal expr);
            public TR Visit(Unary expr);
            public TR Visit(Call expr);
            public TR Visit(Get expr);
            public TR Visit(Set expr);
            public TR Visit(Lambda expr);
            public TR Visit(Variable expr);
            public TR Visit(Assign expr);
        }

        public class Ternary : Expr
        {
            public Expr Left { get; set; }
            public Token Op1 { get; set; }
            public Expr Mid { get; set; }
            public Token Op2 { get; set; }
            public Expr Right { get; set; }

            public Ternary(Expr left, Token op1, Expr mid, Token op2, Expr right)
            {
                Left = left;
                Op1 = op1;
                Mid = mid;
                Op2 = op2;
                Right = right;
            }

            public override TR Accept<TR>(IVisitorExpr<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Binary : Expr
        {
            public Expr Left { get; set; }
            public Token Op { get; set; }
            public Expr Right { get; set; }

            public Binary(Expr left, Token op, Expr right)
            {
                Left = left;
                Op = op;
                Right = right;
            }

            public override TR Accept<TR>(IVisitorExpr<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Logical : Expr
        {
            public Expr Left { get; set; }
            public Token Op { get; set; }
            public Expr Right { get; set; }

            public Logical(Expr left, Token op, Expr right)
            {
                Left = left;
                Op = op;
                Right = right;
            }

            public override TR Accept<TR>(IVisitorExpr<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Grouping : Expr
        {
            public Expr Expr { get; set; }

            public Grouping(Expr expr)
            {
                Expr = expr;
            }

            public override TR Accept<TR>(IVisitorExpr<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Literal : Expr
        {
            public Object Value { get; set; }

            public Literal(Object value)
            {
                Value = value;
            }

            public override TR Accept<TR>(IVisitorExpr<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Unary : Expr
        {
            public Token Op { get; set; }
            public Expr Right { get; set; }

            public Unary(Token op, Expr right)
            {
                Op = op;
                Right = right;
            }

            public override TR Accept<TR>(IVisitorExpr<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Call : Expr
        {
            public Expr Callee { get; set; }
            public Token TraceParen { get; set; }
            public List<Expr> Arguments { get; set; }

            public Call(Expr callee, Token traceparen, List<Expr> arguments)
            {
                Callee = callee;
                TraceParen = traceparen;
                Arguments = arguments;
            }

            public override TR Accept<TR>(IVisitorExpr<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Get : Expr
        {
            public Expr Target { get; set; }
            public Token Name { get; set; }

            public Get(Expr target, Token name)
            {
                Target = target;
                Name = name;
            }

            public override TR Accept<TR>(IVisitorExpr<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Set : Expr
        {
            public Expr Target { get; set; }
            public Token Name { get; set; }
            public Expr Value { get; set; }

            public Set(Expr target, Token name, Expr value)
            {
                Target = target;
                Name = name;
                Value = value;
            }

            public override TR Accept<TR>(IVisitorExpr<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Lambda : Expr
        {
            public Token Name { get; set; }
            public List<Token> Parameters { get; set; }
            public List<Stmt> Body { get; set; }

            public Lambda(Token name, List<Token> parameters, List<Stmt> body)
            {
                Name = name;
                Parameters = parameters;
                Body = body;
            }

            public override TR Accept<TR>(IVisitorExpr<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Variable : Expr
        {
            public Token Name { get; set; }

            public Variable(Token name)
            {
                Name = name;
            }

            public override TR Accept<TR>(IVisitorExpr<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Assign : Expr
        {
            public Token Name { get; set; }
            public Expr Value { get; set; }

            public Assign(Token name, Expr value)
            {
                Name = name;
                Value = value;
            }

            public override TR Accept<TR>(IVisitorExpr<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public abstract TR Accept<TR>(IVisitorExpr<TR> visitor);
    }
}