namespace DotNetLxInterpreter.MiddleGround;

public enum SymanticEnvironmentFlags
{
  None = 0b_0000_0000,
  Loop = 0b_0000_0001,
  Function = 0b_0000_0010,
  Method = 0b_0000_0100,
  Class = 0b_0000_1000,
  Callable = Method | Function
}
