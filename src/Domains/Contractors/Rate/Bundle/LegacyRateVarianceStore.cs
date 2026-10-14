namespace Acme.Workforce.Contractors.Rate.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy rate variances by code.</summary>
public interface ILegacyRateVarianceStore
{
    LegacyRateVariance? Find(string code);
    void Save(LegacyRateVariance item);
    bool Remove(string code);
    List<LegacyRateVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyRateVarianceStore(Dictionary<string, LegacyRateVariance> items, List<string> order) : ILegacyRateVarianceStore
{
    public Dictionary<string, LegacyRateVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyRateVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyRateVariance item)
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

    public List<LegacyRateVariance> List()
    {
        var outValue = new List<LegacyRateVariance>();
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
public sealed class CachedLegacyRateVarianceStore(MemoryLegacyRateVarianceStore inner, LegacyRateVarianceCache cache, int hits, int misses) : ILegacyRateVarianceStore
{
    public MemoryLegacyRateVarianceStore Inner { get; set; } = inner;
    public LegacyRateVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyRateVariance? Find(string code)
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

    public void Save(LegacyRateVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyRateVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy rate variances. Empty fields match all.</summary>
public sealed record LegacyRateVarianceQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyRateVariance item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<LegacyRateVariance> Apply(List<LegacyRateVariance> items)
    {
        var matched = Collections.Filter(items, (LegacyRateVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy rate variance store.</summary>
public static class LegacyRateVarianceStore
{
    public static ILegacyRateVarianceStore NewMemoryLegacyRateVarianceStore()
    {
        return new MemoryLegacyRateVarianceStore(new Dictionary<string, LegacyRateVariance>(), new List<string>());
    }

    public static ILegacyRateVarianceStore NewCachedLegacyRateVarianceStore(MemoryLegacyRateVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyRateVarianceStore(inner, LegacyRateVarianceCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LegacyRateVariance> FindLegacyRateVariances(ILegacyRateVarianceStore store, LegacyRateVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
