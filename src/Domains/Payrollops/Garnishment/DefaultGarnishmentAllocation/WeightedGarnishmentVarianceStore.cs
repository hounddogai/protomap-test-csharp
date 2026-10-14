namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted garnishment variances by code.</summary>
public interface IWeightedGarnishmentVarianceStore
{
    WeightedGarnishmentVariance? Find(string code);
    void Save(WeightedGarnishmentVariance item);
    bool Remove(string code);
    List<WeightedGarnishmentVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedGarnishmentVarianceStore(Dictionary<string, WeightedGarnishmentVariance> items, List<string> order) : IWeightedGarnishmentVarianceStore
{
    public Dictionary<string, WeightedGarnishmentVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedGarnishmentVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedGarnishmentVariance item)
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

    public List<WeightedGarnishmentVariance> List()
    {
        var outValue = new List<WeightedGarnishmentVariance>();
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
public sealed class CachedWeightedGarnishmentVarianceStore(MemoryWeightedGarnishmentVarianceStore inner, WeightedGarnishmentVarianceCache cache, int hits, int misses) : IWeightedGarnishmentVarianceStore
{
    public MemoryWeightedGarnishmentVarianceStore Inner { get; set; } = inner;
    public WeightedGarnishmentVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedGarnishmentVariance? Find(string code)
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

    public void Save(WeightedGarnishmentVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedGarnishmentVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted garnishment variances. Empty fields match all.</summary>
public sealed record WeightedGarnishmentVarianceQuery(string Segment, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedGarnishmentVariance item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedGarnishmentVariance> Apply(List<WeightedGarnishmentVariance> items)
    {
        var matched = Collections.Filter(items, (WeightedGarnishmentVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted garnishment variance store.</summary>
public static class WeightedGarnishmentVarianceStore
{
    public static IWeightedGarnishmentVarianceStore NewMemoryWeightedGarnishmentVarianceStore()
    {
        return new MemoryWeightedGarnishmentVarianceStore(
            new Dictionary<string, WeightedGarnishmentVariance>(),
            new List<string>());
    }

    public static IWeightedGarnishmentVarianceStore NewCachedWeightedGarnishmentVarianceStore(MemoryWeightedGarnishmentVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedGarnishmentVarianceStore(
            inner,
            WeightedGarnishmentVarianceCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedGarnishmentVariance> FindWeightedGarnishmentVariances(IWeightedGarnishmentVarianceStore store, WeightedGarnishmentVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
