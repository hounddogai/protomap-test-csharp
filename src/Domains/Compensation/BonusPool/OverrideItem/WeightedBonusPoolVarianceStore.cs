namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted bonus pool variances by code.</summary>
public interface IWeightedBonusPoolVarianceStore
{
    WeightedBonusPoolVariance? Find(string code);
    void Save(WeightedBonusPoolVariance item);
    bool Remove(string code);
    List<WeightedBonusPoolVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedBonusPoolVarianceStore(Dictionary<string, WeightedBonusPoolVariance> items, List<string> order) : IWeightedBonusPoolVarianceStore
{
    public Dictionary<string, WeightedBonusPoolVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedBonusPoolVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedBonusPoolVariance item)
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

    public List<WeightedBonusPoolVariance> List()
    {
        var outValue = new List<WeightedBonusPoolVariance>();
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
public sealed class CachedWeightedBonusPoolVarianceStore(MemoryWeightedBonusPoolVarianceStore inner, WeightedBonusPoolVarianceCache cache, int hits, int misses) : IWeightedBonusPoolVarianceStore
{
    public MemoryWeightedBonusPoolVarianceStore Inner { get; set; } = inner;
    public WeightedBonusPoolVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedBonusPoolVariance? Find(string code)
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

    public void Save(WeightedBonusPoolVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedBonusPoolVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted bonus pool variances. Empty fields match all.</summary>
public sealed record WeightedBonusPoolVarianceQuery(string Title, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedBonusPoolVariance item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedBonusPoolVariance> Apply(List<WeightedBonusPoolVariance> items)
    {
        var matched = Collections.Filter(items, (WeightedBonusPoolVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted bonus pool variance store.</summary>
public static class WeightedBonusPoolVarianceStore
{
    public static IWeightedBonusPoolVarianceStore NewMemoryWeightedBonusPoolVarianceStore()
    {
        return new MemoryWeightedBonusPoolVarianceStore(
            new Dictionary<string, WeightedBonusPoolVariance>(),
            new List<string>());
    }

    public static IWeightedBonusPoolVarianceStore NewCachedWeightedBonusPoolVarianceStore(MemoryWeightedBonusPoolVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedBonusPoolVarianceStore(
            inner,
            WeightedBonusPoolVarianceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<WeightedBonusPoolVariance> FindWeightedBonusPoolVariances(IWeightedBonusPoolVarianceStore store, WeightedBonusPoolVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
