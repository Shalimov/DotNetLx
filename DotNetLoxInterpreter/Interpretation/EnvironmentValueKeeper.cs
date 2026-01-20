namespace DotNetLoxInterpreter.Interpretation;

internal class EnvironmentValueKeeper
{
  internal class ValueContainer
  {
    private object? _value;

    public bool IsInitialized { get; private set; }
    public object? Value
    {
      get => _value;
      set
      {
        IsInitialized = true;
        _value = value;
      }
    }
  }

  private readonly Dictionary<string, ValueContainer> valueBag = new();
  private readonly List<ValueContainer> valueList = new();

  public ValueContainer this[int scopeIndex] => valueList[scopeIndex];
  public ValueContainer this[string name] => valueBag[name];

  public void Declare(string name)
  {
    var container = new ValueContainer();
    valueBag.Add(name, container);
    valueList.Add(container);
  }

  public bool TryGetValueByKey(string name, out ValueContainer? container)
  {
    return valueBag.TryGetValue(name, out container);
  }
}
