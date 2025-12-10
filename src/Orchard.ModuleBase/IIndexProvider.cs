namespace Orchard.ModuleBase;

public interface IIndexProvider
{
    void Describe(IndexDescriptor descriptor);
}

public class IndexDescriptor
{
    private readonly Dictionary<string, Action<IndexBuilder>> _indexes = new();

    public void For(string indexName, Action<IndexBuilder> describe)
    {
        _indexes[indexName] = describe;
    }

    public IReadOnlyDictionary<string, Action<IndexBuilder>> Build() => _indexes;
}

public class IndexBuilder
{
    private readonly List<string> _fields = new();
    public IndexBuilder Field(string name) { _fields.Add(name); return this; }
    public IReadOnlyList<string> Fields => _fields;
}
