namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted span mappings by code.</summary>
public interface IWeightedSpanMappingStore
{
    WeightedSpanMapping? Find(string code);
    void Save(WeightedSpanMapping item);
    bool Remove(string code);
    List<WeightedSpanMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedSpanMappingStore(Dictionary<string, WeightedSpanMapping> items, List<string> order) : IWeightedSpanMappingStore
{
    public Dictionary<string, WeightedSpanMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedSpanMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedSpanMapping item)
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

    public List<WeightedSpanMapping> List()
    {
        var outValue = new List<WeightedSpanMapping>();
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
public sealed class CachedWeightedSpanMappingStore(MemoryWeightedSpanMappingStore inner, WeightedSpanMappingCache cache, int hits, int misses) : IWeightedSpanMappingStore
{
    public MemoryWeightedSpanMappingStore Inner { get; set; } = inner;
    public WeightedSpanMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedSpanMapping? Find(string code)
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

    public void Save(WeightedSpanMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedSpanMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted span mappings. Empty fields match all.</summary>
public sealed record WeightedSpanMappingQuery(string Department, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedSpanMapping item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedSpanMapping> Apply(List<WeightedSpanMapping> items)
    {
        var matched = Collections.Filter(items, (WeightedSpanMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted span mapping store.</summary>
public static class WeightedSpanMappingStore
{
    public static IWeightedSpanMappingStore NewMemoryWeightedSpanMappingStore()
    {
        return new MemoryWeightedSpanMappingStore(new Dictionary<string, WeightedSpanMapping>(), new List<string>());
    }

    public static IWeightedSpanMappingStore NewCachedWeightedSpanMappingStore(MemoryWeightedSpanMappingStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedSpanMappingStore(inner, WeightedSpanMappingCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<WeightedSpanMapping> FindWeightedSpanMappings(IWeightedSpanMappingStore store, WeightedSpanMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
