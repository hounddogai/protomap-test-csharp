namespace Acme.Workforce.Absence.Balance.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback balance thresholds by code.</summary>
public interface IFallbackBalanceThresholdStore
{
    FallbackBalanceThreshold? Find(string code);
    void Save(FallbackBalanceThreshold item);
    bool Remove(string code);
    List<FallbackBalanceThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackBalanceThresholdStore(Dictionary<string, FallbackBalanceThreshold> items, List<string> order) : IFallbackBalanceThresholdStore
{
    public Dictionary<string, FallbackBalanceThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackBalanceThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackBalanceThreshold item)
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

    public List<FallbackBalanceThreshold> List()
    {
        var outValue = new List<FallbackBalanceThreshold>();
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
public sealed class CachedFallbackBalanceThresholdStore(MemoryFallbackBalanceThresholdStore inner, FallbackBalanceThresholdCache cache, int hits, int misses) : IFallbackBalanceThresholdStore
{
    public MemoryFallbackBalanceThresholdStore Inner { get; set; } = inner;
    public FallbackBalanceThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackBalanceThreshold? Find(string code)
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

    public void Save(FallbackBalanceThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackBalanceThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback balance thresholds. Empty fields match all.</summary>
public sealed record FallbackBalanceThresholdQuery(string Category, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackBalanceThreshold item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<FallbackBalanceThreshold> Apply(List<FallbackBalanceThreshold> items)
    {
        var matched = Collections.Filter(items, (FallbackBalanceThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback balance threshold store.</summary>
public static class FallbackBalanceThresholdStore
{
    public static IFallbackBalanceThresholdStore NewMemoryFallbackBalanceThresholdStore()
    {
        return new MemoryFallbackBalanceThresholdStore(
            new Dictionary<string, FallbackBalanceThreshold>(),
            new List<string>());
    }

    public static IFallbackBalanceThresholdStore NewCachedFallbackBalanceThresholdStore(MemoryFallbackBalanceThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackBalanceThresholdStore(
            inner,
            FallbackBalanceThresholdCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackBalanceThreshold> FindFallbackBalanceThresholds(IFallbackBalanceThresholdStore store, FallbackBalanceThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
