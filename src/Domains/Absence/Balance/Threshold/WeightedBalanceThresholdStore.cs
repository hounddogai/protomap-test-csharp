namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted balance thresholds by code.</summary>
public interface IWeightedBalanceThresholdStore
{
    WeightedBalanceThreshold? Find(string code);
    void Save(WeightedBalanceThreshold item);
    bool Remove(string code);
    List<WeightedBalanceThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedBalanceThresholdStore(Dictionary<string, WeightedBalanceThreshold> items, List<string> order) : IWeightedBalanceThresholdStore
{
    public Dictionary<string, WeightedBalanceThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedBalanceThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedBalanceThreshold item)
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

    public List<WeightedBalanceThreshold> List()
    {
        var outValue = new List<WeightedBalanceThreshold>();
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
public sealed class CachedWeightedBalanceThresholdStore(MemoryWeightedBalanceThresholdStore inner, WeightedBalanceThresholdCache cache, int hits, int misses) : IWeightedBalanceThresholdStore
{
    public MemoryWeightedBalanceThresholdStore Inner { get; set; } = inner;
    public WeightedBalanceThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedBalanceThreshold? Find(string code)
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

    public void Save(WeightedBalanceThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedBalanceThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted balance thresholds. Empty fields match all.</summary>
public sealed record WeightedBalanceThresholdQuery(string Source, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedBalanceThreshold item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedBalanceThreshold> Apply(List<WeightedBalanceThreshold> items)
    {
        var matched = Collections.Filter(items, (WeightedBalanceThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted balance threshold store.</summary>
public static class WeightedBalanceThresholdStore
{
    public static IWeightedBalanceThresholdStore NewMemoryWeightedBalanceThresholdStore()
    {
        return new MemoryWeightedBalanceThresholdStore(
            new Dictionary<string, WeightedBalanceThreshold>(),
            new List<string>());
    }

    public static IWeightedBalanceThresholdStore NewCachedWeightedBalanceThresholdStore(MemoryWeightedBalanceThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedBalanceThresholdStore(
            inner,
            WeightedBalanceThresholdCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<WeightedBalanceThreshold> FindWeightedBalanceThresholds(IWeightedBalanceThresholdStore store, WeightedBalanceThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
