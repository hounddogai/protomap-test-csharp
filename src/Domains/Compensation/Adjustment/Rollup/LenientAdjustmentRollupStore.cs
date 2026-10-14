namespace Acme.Workforce.Compensation.Adjustment.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient adjustment rollups by code.</summary>
public interface ILenientAdjustmentRollupStore
{
    LenientAdjustmentRollup? Find(string code);
    void Save(LenientAdjustmentRollup item);
    bool Remove(string code);
    List<LenientAdjustmentRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientAdjustmentRollupStore(Dictionary<string, LenientAdjustmentRollup> items, List<string> order) : ILenientAdjustmentRollupStore
{
    public Dictionary<string, LenientAdjustmentRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientAdjustmentRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientAdjustmentRollup item)
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

    public List<LenientAdjustmentRollup> List()
    {
        var outValue = new List<LenientAdjustmentRollup>();
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
public sealed class CachedLenientAdjustmentRollupStore(MemoryLenientAdjustmentRollupStore inner, LenientAdjustmentRollupCache cache, int hits, int misses) : ILenientAdjustmentRollupStore
{
    public MemoryLenientAdjustmentRollupStore Inner { get; set; } = inner;
    public LenientAdjustmentRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientAdjustmentRollup? Find(string code)
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

    public void Save(LenientAdjustmentRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientAdjustmentRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient adjustment rollups. Empty fields match all.</summary>
public sealed record LenientAdjustmentRollupQuery(string Country, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(LenientAdjustmentRollup item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientAdjustmentRollup> Apply(List<LenientAdjustmentRollup> items)
    {
        var matched = Collections.Filter(items, (LenientAdjustmentRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient adjustment rollup store.</summary>
public static class LenientAdjustmentRollupStore
{
    public static ILenientAdjustmentRollupStore NewMemoryLenientAdjustmentRollupStore()
    {
        return new MemoryLenientAdjustmentRollupStore(
            new Dictionary<string, LenientAdjustmentRollup>(),
            new List<string>());
    }

    public static ILenientAdjustmentRollupStore NewCachedLenientAdjustmentRollupStore(MemoryLenientAdjustmentRollupStore inner, int ttl, IClock clock)
    {
        return new CachedLenientAdjustmentRollupStore(
            inner,
            LenientAdjustmentRollupCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LenientAdjustmentRollup> FindLenientAdjustmentRollups(ILenientAdjustmentRollupStore store, LenientAdjustmentRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
