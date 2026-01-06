/* IMPORTANT: Generated Code Do Not Amend */
namespace DotNetLoxInterpreter
{
    public abstract class Stmt
    {
        public interface IVisitorStmt<out TR>
        {
            public TR Visit(Block expr);
            public TR Visit(If expr);
            public TR Visit(While expr);
            public TR Visit(Expression expr);
            public TR Visit(Print expr);
            public TR Visit(Var expr);
        }

        public class Block : Stmt
        {
            public List<Stmt> Statements { get; set; }

            public Block(List<Stmt> statements)
            {
                Statements = statements;
            }

            public override TR Accept<TR>(IVisitorStmt<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class If : Stmt
        {
            public Expr Condition { get; set; }
            public Stmt ThenBranch { get; set; }
            public Stmt? ElseBranch { get; set; }

            public If(Expr condition, Stmt thenbranch, Stmt? elsebranch)
            {
                Condition = condition;
                ThenBranch = thenbranch;
                ElseBranch = elsebranch;
            }

            public override TR Accept<TR>(IVisitorStmt<TR> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class While : Stmt
        {
            public Expr Condition { get; set; }
            public Stmt WhileBody { get; set; }

            public While(Expr condition, Stmt whilebody)
            {
                Condition = condition;
                WhileBody = whilebody;
            }

            public override TR Accept<TR>(IVisitorStmt<TR> visitor)
            {
                return visitor.Visit(this);
            }
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