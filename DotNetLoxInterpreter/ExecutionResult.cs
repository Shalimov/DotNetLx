namespace DotNetLoxInterpreter;

public sealed class ExecutionResult : IComparable<ExecutionResult>
{
    public static readonly ExecutionResult Normal = new("Normal", 0);
    public static readonly ExecutionResult Break = new("Break", 1);
    
    public string Name { get; }
    public int Value { get; }
    
    private ExecutionResult(string name, int value)
    {
        Name = name;
        Value = value;
    }

    public int CompareTo(ExecutionResult? other)
    {
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }

    public override string ToString() => Name;

    public override bool Equals(object? obj) => obj is ExecutionResult other && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ExecutionResult? left, ExecutionResult? right) =>
        left?.Value == right?.Value;

    public static bool operator !=(ExecutionResult? left, ExecutionResult? right) =>
        !(left == right);
}
