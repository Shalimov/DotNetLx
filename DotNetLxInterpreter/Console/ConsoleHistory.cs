public class ConsoleHistory
{
    private readonly List<string> _history = new List<string>();
    private int _historyIndex = -1;
    private string _currentInput = string.Empty;
    private int _maxHistory = 10;
    private readonly string _commandStart;

    public ConsoleHistory(string commandStart = "> ")
    {
        _commandStart = commandStart;
    }

    public int MaxHistory
    {
        get => _maxHistory;
        set => _maxHistory = value;
    }

    public string ReadLine()
    {
        // Write the prompt
        Console.Write(_commandStart);
        int promptLength = _commandStart.Length;
        
        _currentInput = string.Empty;
        _historyIndex = _history.Count;
        int cursorPosition = 0;

        while (true)
        {
            var key = Console.ReadKey(intercept: true);

            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    if (!string.IsNullOrWhiteSpace(_currentInput))
                    {
                        AddToHistory(_currentInput);
                    }
                    return _currentInput;

                case ConsoleKey.UpArrow:
                    if (_historyIndex > 0)
                    {
                        _historyIndex--;
                        ReplaceCurrentLine(_history[_historyIndex], ref cursorPosition, promptLength);
                    }
                    break;

                case ConsoleKey.DownArrow:
                    if (_historyIndex < _history.Count - 1)
                    {
                        _historyIndex++;
                        ReplaceCurrentLine(_history[_historyIndex], ref cursorPosition, promptLength);
                    }
                    else if (_historyIndex == _history.Count - 1)
                    {
                        _historyIndex = _history.Count;
                        ReplaceCurrentLine(string.Empty, ref cursorPosition, promptLength);
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (cursorPosition > 0)
                    {
                        cursorPosition--;
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (cursorPosition < _currentInput.Length)
                    {
                        cursorPosition++;
                        Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                    }
                    break;

                case ConsoleKey.Home:
                    Console.SetCursorPosition(promptLength, Console.CursorTop);
                    cursorPosition = 0;
                    break;

                case ConsoleKey.End:
                    Console.SetCursorPosition(promptLength + _currentInput.Length, Console.CursorTop);
                    cursorPosition = _currentInput.Length;
                    break;

                case ConsoleKey.Backspace:
                    if (cursorPosition > 0)
                    {
                        _currentInput = _currentInput.Remove(cursorPosition - 1, 1);
                        cursorPosition--;
                        RedrawLine(cursorPosition, promptLength);
                    }
                    break;

                case ConsoleKey.Delete:
                    if (cursorPosition < _currentInput.Length)
                    {
                        _currentInput = _currentInput.Remove(cursorPosition, 1);
                        RedrawLine(cursorPosition, promptLength);
                    }
                    break;

                case ConsoleKey.Tab:
                    // Optional: Add tab completion support here
                    break;

                case ConsoleKey.Escape:
                    // Clear current input
                    _currentInput = string.Empty;
                    cursorPosition = 0;
                    RedrawLine(cursorPosition, promptLength);
                    break;

                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        _currentInput = _currentInput.Insert(cursorPosition, key.KeyChar.ToString());
                        cursorPosition++;
                        RedrawLine(cursorPosition, promptLength);
                    }
                    break;
            }
        }
    }

    private void ReplaceCurrentLine(string newText, ref int cursorPosition, int promptLength)
    {
        // Move to start of input (after prompt)
        Console.SetCursorPosition(promptLength, Console.CursorTop);
        
        // Clear the rest of the line
        Console.Write(new string(' ', Console.WindowWidth - promptLength - 1));
        Console.SetCursorPosition(promptLength, Console.CursorTop);

        // Write new text
        _currentInput = newText;
        Console.Write(_currentInput);
        cursorPosition = _currentInput.Length;
    }

    private void RedrawLine(int cursorPosition, int promptLength)
    {
        int currentLine = Console.CursorTop;

        // Go to start of input (after prompt)
        Console.SetCursorPosition(promptLength, currentLine);
        
        // Clear and redraw
        Console.Write(_currentInput);
        Console.Write(new string(' ', Console.WindowWidth - promptLength - _currentInput.Length - 1));
        
        // Restore cursor position
        Console.SetCursorPosition(promptLength + cursorPosition, currentLine);
    }

    private void AddToHistory(string command)
    {
        // Don't add duplicates of the last command
        if (_history.Count > 0 && _history[_history.Count - 1] == command)
            return;

        _history.Add(command);

        // Keep only last N commands
        if (_history.Count > _maxHistory)
            _history.RemoveAt(0);
    }

    public void ClearHistory()
    {
        _history.Clear();
        _historyIndex = -1;
    }

    public IReadOnlyList<string> GetHistory() => _history.AsReadOnly();
}