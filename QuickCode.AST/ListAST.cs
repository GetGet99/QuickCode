using System.Collections;

namespace QuickCode.AST;

public record class ListAST<T>(List<T> Values) : AST, IList<T>
{
    public ListAST() : this(new List<T>()) { }
    public T this[int index] { get => ((IList<T>)Values)[index]; set => ((IList<T>)Values)[index] = value; }

    public int Count => ((ICollection<T>)Values).Count;

    public bool IsReadOnly => ((ICollection<T>)Values).IsReadOnly;

    public void Add(T item)
    {
        ((ICollection<T>)Values).Add(item);
    }

    public void Clear()
    {
        ((ICollection<T>)Values).Clear();
    }

    public bool Contains(T item)
    {
        return ((ICollection<T>)Values).Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        ((ICollection<T>)Values).CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)Values).GetEnumerator();
    }

    public int IndexOf(T item)
    {
        return ((IList<T>)Values).IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        ((IList<T>)Values).Insert(index, item);
    }

    public bool Remove(T item)
    {
        return ((ICollection<T>)Values).Remove(item);
    }

    public void RemoveAt(int index)
    {
        ((IList<T>)Values).RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Values).GetEnumerator();
    }
}
