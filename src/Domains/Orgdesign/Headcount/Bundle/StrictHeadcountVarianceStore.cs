namespace Acme.Workforce.Orgdesign.Headcount.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict headcount variances by code.</summary>
public interface IStrictHeadcountVarianceStore
{
    StrictHeadcountVariance? Find(string code);
    void Save(StrictHeadcountVariance item);
    bool Remove(string code);
    List<StrictHeadcountVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictHeadcountVarianceStore(Dictionary<string, StrictHeadcountVariance> items, List<string> order) : IStrictHeadcountVarianceStore
{
    public Dictionary<string, StrictHeadcountVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictHeadcountVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictHeadcountVariance item)
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

    public List<StrictHeadcountVariance> List()
    {
        var outValue = new List<StrictHeadcountVariance>();
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
public sealed class CachedStrictHeadcountVarianceStore(MemoryStrictHeadcountVarianceStore inner, StrictHeadcountVarianceCache cache, int hits, int misses) : IStrictHeadcountVarianceStore
{
    public MemoryStrictHeadcountVarianceStore Inner { get; set; } = inner;
    public StrictHeadcountVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictHeadcountVariance? Find(string code)
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

    public void Save(StrictHeadcountVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictHeadcountVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict headcount variances. Empty fields match all.</summary>
public sealed record StrictHeadcountVarianceQuery(string Unit, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(StrictHeadcountVariance item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictHeadcountVariance> Apply(List<StrictHeadcountVariance> items)
    {
        var matched = Collections.Filter(items, (StrictHeadcountVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict headcount variance store.</summary>
public static class StrictHeadcountVarianceStore
{
    public static IStrictHeadcountVarianceStore NewMemoryStrictHeadcountVarianceStore()
    {
        return new MemoryStrictHeadcountVarianceStore(
            new Dictionary<string, StrictHeadcountVariance>(),
            new List<string>());
    }

    public static IStrictHeadcountVarianceStore NewCachedStrictHeadcountVarianceStore(MemoryStrictHeadcountVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedStrictHeadcountVarianceStore(
            inner,
            StrictHeadcountVarianceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<StrictHeadcountVariance> FindStrictHeadcountVariances(IStrictHeadcountVarianceStore store, StrictHeadcountVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
