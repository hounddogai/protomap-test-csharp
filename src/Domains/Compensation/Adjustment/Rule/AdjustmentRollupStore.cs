namespace Acme.Workforce.Compensation.Adjustment.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment rollups by code.</summary>
public interface IAdjustmentRollupStore
{
    AdjustmentRollup? Find(string code);
    void Save(AdjustmentRollup item);
    bool Remove(string code);
    List<AdjustmentRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentRollupStore(Dictionary<string, AdjustmentRollup> items, List<string> order) : IAdjustmentRollupStore
{
    public Dictionary<string, AdjustmentRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentRollup item)
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

    public List<AdjustmentRollup> List()
    {
        var outValue = new List<AdjustmentRollup>();
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
public sealed class CachedAdjustmentRollupStore(MemoryAdjustmentRollupStore inner, AdjustmentRollupCache cache, int hits, int misses) : IAdjustmentRollupStore
{
    public MemoryAdjustmentRollupStore Inner { get; set; } = inner;
    public AdjustmentRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentRollup? Find(string code)
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

    public void Save(AdjustmentRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment rollups. Empty fields match all.</summary>
public sealed record AdjustmentRollupQuery(string Source, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentRollup item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentRollup> Apply(List<AdjustmentRollup> items)
    {
        var matched = Collections.Filter(items, (AdjustmentRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment rollup store.</summary>
public static class AdjustmentRollupStore
{
    public static IAdjustmentRollupStore NewMemoryAdjustmentRollupStore()
    {
        return new MemoryAdjustmentRollupStore(new Dictionary<string, AdjustmentRollup>(), new List<string>());
    }

    public static IAdjustmentRollupStore NewCachedAdjustmentRollupStore(MemoryAdjustmentRollupStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentRollupStore(inner, AdjustmentRollupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AdjustmentRollup> FindAdjustmentRollups(IAdjustmentRollupStore store, AdjustmentRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
