namespace DotNetLxInterpreter.MiddleGround;

[Flags]
public enum SymanticEnvironmentFlags
{
  None = 0b_0000_0000,
  Loop = 0b_0000_0001,
  Function = 0b_0000_0010,
  Method = 0b_0000_0100,
  Initializer = 0b_0000_1000,
  SubClass = 0b_0100_0000,
  Class = 0b_1000_0000,

  Callable = Method | Function | Initializer
}
