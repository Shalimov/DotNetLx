namespace DotNetLxInterpreter.FrontEnd;

[Flags]
public enum FnModifier
{
  None = 0b_0000_0000,
  Property = 0b_0000_0001,
  Static = 0b_0000_0010,
  Protocol = 0b_1000_0000,
}
