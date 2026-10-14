namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted warranty mappings by code.</summary>
public interface IWeightedWarrantyMappingStore
{
    WeightedWarrantyMapping? Find(string code);
    void Save(WeightedWarrantyMapping item);
    bool Remove(string code);
    List<WeightedWarrantyMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedWarrantyMappingStore(Dictionary<string, WeightedWarrantyMapping> items, List<string> order) : IWeightedWarrantyMappingStore
{
    public Dictionary<string, WeightedWarrantyMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedWarrantyMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedWarrantyMapping item)
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

    public List<WeightedWarrantyMapping> List()
    {
        var outValue = new List<WeightedWarrantyMapping>();
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
public sealed class CachedWeightedWarrantyMappingStore(MemoryWeightedWarrantyMappingStore inner, WeightedWarrantyMappingCache cache, int hits, int misses) : IWeightedWarrantyMappingStore
{
    public MemoryWeightedWarrantyMappingStore Inner { get; set; } = inner;
    public WeightedWarrantyMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedWarrantyMapping? Find(string code)
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

    public void Save(WeightedWarrantyMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedWarrantyMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted warranty mappings. Empty fields match all.</summary>
public sealed record WeightedWarrantyMappingQuery(string Label, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedWarrantyMapping item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedWarrantyMapping> Apply(List<WeightedWarrantyMapping> items)
    {
        var matched = Collections.Filter(items, (WeightedWarrantyMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted warranty mapping store.</summary>
public static class WeightedWarrantyMappingStore
{
    public static IWeightedWarrantyMappingStore NewMemoryWeightedWarrantyMappingStore()
    {
        return new MemoryWeightedWarrantyMappingStore(
            new Dictionary<string, WeightedWarrantyMapping>(),
            new List<string>());
    }

    public static IWeightedWarrantyMappingStore NewCachedWeightedWarrantyMappingStore(MemoryWeightedWarrantyMappingStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedWarrantyMappingStore(
            inner,
            WeightedWarrantyMappingCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<WeightedWarrantyMapping> FindWeightedWarrantyMappings(IWeightedWarrantyMappingStore store, WeightedWarrantyMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
