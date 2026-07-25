namespace TagBites.Collections;

internal sealed class ReadOnlyTypeSet(ISet<Type> source) : ISet<Type>
{
    public int Count => source.Count;
    public bool IsReadOnly => true;


    public bool Contains(Type item) => source.Contains(item);
    public void CopyTo(Type[] array, int arrayIndex) => source.CopyTo(array, arrayIndex);
    public IEnumerator<Type> GetEnumerator() => source.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool IsProperSubsetOf(IEnumerable<Type> other) => source.IsProperSubsetOf(other);
    public bool IsProperSupersetOf(IEnumerable<Type> other) => source.IsProperSupersetOf(other);
    public bool IsSubsetOf(IEnumerable<Type> other) => source.IsSubsetOf(other);
    public bool IsSupersetOf(IEnumerable<Type> other) => source.IsSupersetOf(other);
    public bool Overlaps(IEnumerable<Type> other) => source.Overlaps(other);
    public bool SetEquals(IEnumerable<Type> other) => source.SetEquals(other);

    bool ISet<Type>.Add(Type item) => throw new NotSupportedException();
    void ICollection<Type>.Add(Type item) => throw new NotSupportedException();
    public void Clear() => throw new NotSupportedException();
    public bool Remove(Type item) => throw new NotSupportedException();
    public void ExceptWith(IEnumerable<Type> other) => throw new NotSupportedException();
    public void IntersectWith(IEnumerable<Type> other) => throw new NotSupportedException();
    public void SymmetricExceptWith(IEnumerable<Type> other) => throw new NotSupportedException();
    public void UnionWith(IEnumerable<Type> other) => throw new NotSupportedException();
}
