namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict bonus pool variances by code.</summary>
public interface IStrictBonusPoolVarianceStore
{
    StrictBonusPoolVariance? Find(string code);
    void Save(StrictBonusPoolVariance item);
    bool Remove(string code);
    List<StrictBonusPoolVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictBonusPoolVarianceStore(Dictionary<string, StrictBonusPoolVariance> items, List<string> order) : IStrictBonusPoolVarianceStore
{
    public Dictionary<string, StrictBonusPoolVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictBonusPoolVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictBonusPoolVariance item)
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

    public List<StrictBonusPoolVariance> List()
    {
        var outValue = new List<StrictBonusPoolVariance>();
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
public sealed class CachedStrictBonusPoolVarianceStore(MemoryStrictBonusPoolVarianceStore inner, StrictBonusPoolVarianceCache cache, int hits, int misses) : IStrictBonusPoolVarianceStore
{
    public MemoryStrictBonusPoolVarianceStore Inner { get; set; } = inner;
    public StrictBonusPoolVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictBonusPoolVariance? Find(string code)
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

    public void Save(StrictBonusPoolVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictBonusPoolVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict bonus pool variances. Empty fields match all.</summary>
public sealed record StrictBonusPoolVarianceQuery(string Label, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(StrictBonusPoolVariance item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictBonusPoolVariance> Apply(List<StrictBonusPoolVariance> items)
    {
        var matched = Collections.Filter(items, (StrictBonusPoolVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict bonus pool variance store.</summary>
public static class StrictBonusPoolVarianceStore
{
    public static IStrictBonusPoolVarianceStore NewMemoryStrictBonusPoolVarianceStore()
    {
        return new MemoryStrictBonusPoolVarianceStore(
            new Dictionary<string, StrictBonusPoolVariance>(),
            new List<string>());
    }

    public static IStrictBonusPoolVarianceStore NewCachedStrictBonusPoolVarianceStore(MemoryStrictBonusPoolVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedStrictBonusPoolVarianceStore(
            inner,
            StrictBonusPoolVarianceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<StrictBonusPoolVariance> FindStrictBonusPoolVariances(IStrictBonusPoolVarianceStore store, StrictBonusPoolVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
