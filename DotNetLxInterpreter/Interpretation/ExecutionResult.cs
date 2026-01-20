namespace DotNetLxInterpreter.Interpretation;

public sealed class ExecutionResult : IComparable<ExecutionResult>
{
    public static readonly ExecutionResult Normal = new("Normal", 0);
    public static readonly ExecutionResult Break = new("Break", 1);
    public static readonly ExecutionResult Continue = new("Continue", 2);
    public static readonly ExecutionResult Return = new("Return", 3);
    public static ExecutionResult ReturnWithPayload(object? payload) => new (Return.Name, Return.Value, payload);

    public string Name { get; }
    public int Value { get; }
    public object? Payload { get; private set; }

    private ExecutionResult(string name, int value, object? payload = null)
    {
        Name = name;
        Value = value;
        Payload = payload;
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
