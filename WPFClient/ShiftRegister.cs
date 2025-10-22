using System;
using System.Collections.Generic;

public class ShiftRegister<T>
{
    private readonly Queue<T> _items;
    public int Capacity { get; }

    public ShiftRegister(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Kapazität muss größer als null sein.");
        }
        Capacity = capacity;
        _items = new Queue<T>(capacity);
    }

    public void Add(T item)
    {
        if (_items.Count >= Capacity)
        {
            _items.Dequeue();
        }
        _items.Enqueue(item);
    }

    public IEnumerable<T> GetAll()
    {
        return _items;
    }

    public T[] ToArray()
    {
        return _items.ToArray();
    }
}