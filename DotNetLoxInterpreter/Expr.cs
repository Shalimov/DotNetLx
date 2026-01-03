/* IMPORTANT: Generated Code Do Not Amend */
namespace DotNetLoxInterpreter
{
    public abstract class Expr
    {
        public interface IVisitorExpr<out TR>
        {
            public TR Visit(Ternary expr);
            public TR Visit(Binary expr);
            public TR Visit(Grouping expr);
            public TR Visit(Literal expr);
            public TR Visit(Unary expr);
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