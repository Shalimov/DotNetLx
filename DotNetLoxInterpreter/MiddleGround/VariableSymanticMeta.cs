namespace DotNetLoxInterpreter.MiddleGround;

public class VariableSymanticMeta
{
  public int ScopeIndex { get; set; }
  public bool IsDefined { get; set; }
  public bool IsUsed { get; set; }
}
