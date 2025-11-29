namespace DotNetLoxInterpreter
{
    public abstract class Expr
    {
        public interface IVisitor<out TR>
        {
            public TR Visit(Binary expr);
            public TR Visit(Grouping expr);
            public TR Visit(Literal expr);
            public TR Visit(Unary expr);
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

            public override TR Accept<TR>(IVisitor<TR> visitor)
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

            public override TR Accept<TR>(IVisitor<TR> visitor)
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

            public override TR Accept<TR>(IVisitor<TR> visitor)
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

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public abstract TR Accept<TR>(IVisitor<TR> visitor);
    }
}