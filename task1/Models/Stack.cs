namespace task1.Models;

public class Stack<TElement>
{
    private class StackItem
    {
        internal required TElement? Element;
        internal required StackItem? Previous;
    }

    private StackItem? _tail;

    public long Length
    {
        get;
        private set;
    }
    
    public bool IsEmpty => Length == 0;
    
    public TElement? TailElement => _tail == null ? default : _tail.Element;

    public TElement? Pop()
    {
        if (_tail == null)
        {
            return default;
        }
        
        var element = _tail.Element;
        _tail = _tail.Previous;
        Length--;

        return element;
    }
    
    public void Append(TElement? element)
    {
        var newTail = new StackItem { Element = element, Previous = _tail };
        
        _tail = newTail;
        Length++;
    }

    public void Clear()
    {
        _tail = null;
        Length = 0;
    }
}