using DotNetLoxInterpreter.Exceptions;

namespace DotNetLoxInterpreter;

internal static class LxRuntimeAssertions
{
  internal static void AssertNumberOperand(Token @operator, object? operand1)
  {
    if (operand1 is double) return;

    throw new LxRuntimeException("Operand must be number", @operator);
  }

  internal static void AssertNumberOperands(Token @operator, object? operand1, object? operand2)
  {
    if (operand1 is double && operand2 is double) return;

    throw new LxRuntimeException("Operands must be numbers", @operator);
  }

  internal static void AssertSameTypeOperands(Token @operator, object? operand1, object? operand2)
  {
    if (operand1 is null && operand2 is null) return;
    
    if (operand1 is null)
    {
      var type = operand2!.GetType();
      throw new LxRuntimeException($"Operands must be {type.Name.ToLower()}", @operator);
    }

    if (operand2 is null)
    {
      var type = operand1!.GetType();
      throw new LxRuntimeException($"Operands must be {type.Name.ToLower()}", @operator);
    }

    var op1Type = operand1.GetType();
    var op2Type = operand2.GetType();

    if (op1Type == op2Type) return;

    throw new LxRuntimeException($"Operands must be the same type, while now operand 1 is {op1Type.Name.ToLower()} and operand 2 is {op2Type.Name.ToLower()}", @operator);
  }

  internal static void AssertNonNullDivisor(Token @operator, double divisor)
  {
    if (divisor == 0.0) throw new LxRuntimeException("Divisor should not be zero.", @operator);
  }
}
