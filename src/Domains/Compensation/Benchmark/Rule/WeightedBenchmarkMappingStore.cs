namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted benchmark mappings by code.</summary>
public interface IWeightedBenchmarkMappingStore
{
    WeightedBenchmarkMapping? Find(string code);
    void Save(WeightedBenchmarkMapping item);
    bool Remove(string code);
    List<WeightedBenchmarkMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedBenchmarkMappingStore(Dictionary<string, WeightedBenchmarkMapping> items, List<string> order) : IWeightedBenchmarkMappingStore
{
    public Dictionary<string, WeightedBenchmarkMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedBenchmarkMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedBenchmarkMapping item)
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

    public List<WeightedBenchmarkMapping> List()
    {
        var outValue = new List<WeightedBenchmarkMapping>();
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
public sealed class CachedWeightedBenchmarkMappingStore(MemoryWeightedBenchmarkMappingStore inner, WeightedBenchmarkMappingCache cache, int hits, int misses) : IWeightedBenchmarkMappingStore
{
    public MemoryWeightedBenchmarkMappingStore Inner { get; set; } = inner;
    public WeightedBenchmarkMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedBenchmarkMapping? Find(string code)
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

    public void Save(WeightedBenchmarkMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedBenchmarkMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted benchmark mappings. Empty fields match all.</summary>
public sealed record WeightedBenchmarkMappingQuery(string Country, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedBenchmarkMapping item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedBenchmarkMapping> Apply(List<WeightedBenchmarkMapping> items)
    {
        var matched = Collections.Filter(items, (WeightedBenchmarkMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted benchmark mapping store.</summary>
public static class WeightedBenchmarkMappingStore
{
    public static IWeightedBenchmarkMappingStore NewMemoryWeightedBenchmarkMappingStore()
    {
        return new MemoryWeightedBenchmarkMappingStore(
            new Dictionary<string, WeightedBenchmarkMapping>(),
            new List<string>());
    }

    public static IWeightedBenchmarkMappingStore NewCachedWeightedBenchmarkMappingStore(MemoryWeightedBenchmarkMappingStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedBenchmarkMappingStore(
            inner,
            WeightedBenchmarkMappingCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<WeightedBenchmarkMapping> FindWeightedBenchmarkMappings(IWeightedBenchmarkMappingStore store, WeightedBenchmarkMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
