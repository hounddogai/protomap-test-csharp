namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark mappings by code.</summary>
public interface IBenchmarkMappingStore
{
    BenchmarkMapping? Find(string code);
    void Save(BenchmarkMapping item);
    bool Remove(string code);
    List<BenchmarkMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkMappingStore(Dictionary<string, BenchmarkMapping> items, List<string> order) : IBenchmarkMappingStore
{
    public Dictionary<string, BenchmarkMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkMapping item)
    {
        if (!Items.ContainsKey(item.Code))
        {
            Order.Add(item.Code);
        }
        Items[item.Code] = item;
    }

    public bool Remove(string code)
    {
        if (!Items.ContainsKey(code))
        {
            return false;
        }
        Items.Remove(code);
        var kept = new List<string>();
        foreach (var existing in Order)
        {
            if (existing != code)
            {
                kept.Add(existing);
            }
        }
        Order = kept;
        return true;
    }

    public List<BenchmarkMapping> List()
    {
        var outValue = new List<BenchmarkMapping>();
        foreach (var key in Order)
        {
            if (!Items.TryGetValue(key, out var found))
            {
                continue;
            }
            outValue.Add(found);
        }
        return outValue;
    }
}

/// <summary>Serves reads from a short-lived cache in front of another store.</summary>
public sealed class CachedBenchmarkMappingStore(MemoryBenchmarkMappingStore inner, BenchmarkMappingCache cache, int hits, int misses) : IBenchmarkMappingStore
{
    public MemoryBenchmarkMappingStore Inner { get; set; } = inner;
    public BenchmarkMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkMapping? Find(string code)
    {
        if (Cache.Get(code) is not { } hit)
        {
            Misses += 1;
            if (Inner.Find(code) is not { } found)
            {
                return null;
            }
            Cache.Put(code, found);
            return found;
        }
        Hits += 1;
        return hit;
    }

    public void Save(BenchmarkMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark mappings. Empty fields match all.</summary>
public sealed record BenchmarkMappingQuery(string Name, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkMapping item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkMapping> Apply(List<BenchmarkMapping> items)
    {
        var matched = Collections.Filter(items, (BenchmarkMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark mapping store.</summary>
public static class BenchmarkMappingStore
{
    public static IBenchmarkMappingStore NewMemoryBenchmarkMappingStore()
    {
        return new MemoryBenchmarkMappingStore(new Dictionary<string, BenchmarkMapping>(), new List<string>());
    }

    public static IBenchmarkMappingStore NewCachedBenchmarkMappingStore(MemoryBenchmarkMappingStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkMappingStore(inner, BenchmarkMappingCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BenchmarkMapping> FindBenchmarkMappings(IBenchmarkMappingStore store, BenchmarkMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
