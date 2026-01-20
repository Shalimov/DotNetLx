namespace DotNetLxInterpreter.Interpretation;

public class LxClass(string name)
{
  public string Name => name;

  public override string ToString() => name;
}
