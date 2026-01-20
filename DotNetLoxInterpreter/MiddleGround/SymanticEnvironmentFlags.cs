namespace DotNetLoxInterpreter.MiddleGround;

public enum SymanticEnvironmentFlags
{
  None = 0b_0000_0000,
  Loop = 0b_0000_0001,
  Function = 0b_0000_0010,
}
