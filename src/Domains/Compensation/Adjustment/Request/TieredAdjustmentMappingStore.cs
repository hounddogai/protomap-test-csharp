namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered adjustment mappings by code.</summary>
public interface ITieredAdjustmentMappingStore
{
    TieredAdjustmentMapping? Find(string code);
    void Save(TieredAdjustmentMapping item);
    bool Remove(string code);
    List<TieredAdjustmentMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredAdjustmentMappingStore(Dictionary<string, TieredAdjustmentMapping> items, List<string> order) : ITieredAdjustmentMappingStore
{
    public Dictionary<string, TieredAdjustmentMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredAdjustmentMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredAdjustmentMapping item)
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

    public List<TieredAdjustmentMapping> List()
    {
        var outValue = new List<TieredAdjustmentMapping>();
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
public sealed class CachedTieredAdjustmentMappingStore(MemoryTieredAdjustmentMappingStore inner, TieredAdjustmentMappingCache cache, int hits, int misses) : ITieredAdjustmentMappingStore
{
    public MemoryTieredAdjustmentMappingStore Inner { get; set; } = inner;
    public TieredAdjustmentMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredAdjustmentMapping? Find(string code)
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

    public void Save(TieredAdjustmentMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredAdjustmentMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered adjustment mappings. Empty fields match all.</summary>
public sealed record TieredAdjustmentMappingQuery(string Name, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(TieredAdjustmentMapping item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredAdjustmentMapping> Apply(List<TieredAdjustmentMapping> items)
    {
        var matched = Collections.Filter(items, (TieredAdjustmentMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered adjustment mapping store.</summary>
public static class TieredAdjustmentMappingStore
{
    public static ITieredAdjustmentMappingStore NewMemoryTieredAdjustmentMappingStore()
    {
        return new MemoryTieredAdjustmentMappingStore(
            new Dictionary<string, TieredAdjustmentMapping>(),
            new List<string>());
    }

    public static ITieredAdjustmentMappingStore NewCachedTieredAdjustmentMappingStore(MemoryTieredAdjustmentMappingStore inner, int ttl, IClock clock)
    {
        return new CachedTieredAdjustmentMappingStore(
            inner,
            TieredAdjustmentMappingCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<TieredAdjustmentMapping> FindTieredAdjustmentMappings(ITieredAdjustmentMappingStore store, TieredAdjustmentMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
