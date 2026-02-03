namespace DotNetLxInterpreter.FrontEnd;

[Flags]
public enum ClassModifier
{
  None = 0b_0000_0000,
  Super = 0b_0000_0001,
  Inner = 0b_0000_0010
}
