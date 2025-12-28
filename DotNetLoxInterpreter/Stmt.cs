/* IMPORTANT: Generated Code Do Not Amend */
namespace DotNetLoxInterpreter
{
    public abstract class Stmt
    {
        public interface IVisitorStmt<out TR>
        {
            public TR Visit(Expression expr);
            public TR Visit(Print expr);
            public TR Visit(Var expr);
        }

        public class Expression : Stmt
        {
            public Expr Expr { get; set; }

            public Expression(Expr expr)
            {
                Expr = expr;
            }

            public override TR Accept<TR>(IVisitorStmt<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Print : Stmt
        {
            public Expr Value { get; set; }

            public Print(Expr value)
            {
                Value = value;
            }

            public override TR Accept<TR>(IVisitorStmt<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Var : Stmt
        {
            public Token Name { get; set; }
            public Expr Initializer { get; set; }

            public Var(Token name, Expr initializer)
            {
                Name = name;
                Initializer = initializer;
            }

            public override TR Accept<TR>(IVisitorStmt<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public abstract TR Accept<TR>(IVisitorStmt<TR> visitor);
    }
}