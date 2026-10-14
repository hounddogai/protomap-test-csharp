namespace Acme.Workforce.Absence.Balance.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback balance adjustments by code.</summary>
public interface IFallbackBalanceAdjustmentStore
{
    FallbackBalanceAdjustment? Find(string code);
    void Save(FallbackBalanceAdjustment item);
    bool Remove(string code);
    List<FallbackBalanceAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackBalanceAdjustmentStore(Dictionary<string, FallbackBalanceAdjustment> items, List<string> order) : IFallbackBalanceAdjustmentStore
{
    public Dictionary<string, FallbackBalanceAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackBalanceAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackBalanceAdjustment item)
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

    public List<FallbackBalanceAdjustment> List()
    {
        var outValue = new List<FallbackBalanceAdjustment>();
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
public sealed class CachedFallbackBalanceAdjustmentStore(MemoryFallbackBalanceAdjustmentStore inner, FallbackBalanceAdjustmentCache cache, int hits, int misses) : IFallbackBalanceAdjustmentStore
{
    public MemoryFallbackBalanceAdjustmentStore Inner { get; set; } = inner;
    public FallbackBalanceAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackBalanceAdjustment? Find(string code)
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

    public void Save(FallbackBalanceAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackBalanceAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback balance adjustments. Empty fields match all.</summary>
public sealed record FallbackBalanceAdjustmentQuery(string Region, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackBalanceAdjustment item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<FallbackBalanceAdjustment> Apply(List<FallbackBalanceAdjustment> items)
    {
        var matched = Collections.Filter(items, (FallbackBalanceAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback balance adjustment store.</summary>
public static class FallbackBalanceAdjustmentStore
{
    public static IFallbackBalanceAdjustmentStore NewMemoryFallbackBalanceAdjustmentStore()
    {
        return new MemoryFallbackBalanceAdjustmentStore(
            new Dictionary<string, FallbackBalanceAdjustment>(),
            new List<string>());
    }

    public static IFallbackBalanceAdjustmentStore NewCachedFallbackBalanceAdjustmentStore(MemoryFallbackBalanceAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackBalanceAdjustmentStore(
            inner,
            FallbackBalanceAdjustmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackBalanceAdjustment> FindFallbackBalanceAdjustments(IFallbackBalanceAdjustmentStore store, FallbackBalanceAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
