namespace Acme.Workforce.Compensation.EquityGrant.GrantRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled equity grant variances by code.</summary>
public interface IScheduledEquityGrantVarianceStore
{
    ScheduledEquityGrantVariance? Find(string code);
    void Save(ScheduledEquityGrantVariance item);
    bool Remove(string code);
    List<ScheduledEquityGrantVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledEquityGrantVarianceStore(Dictionary<string, ScheduledEquityGrantVariance> items, List<string> order) : IScheduledEquityGrantVarianceStore
{
    public Dictionary<string, ScheduledEquityGrantVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledEquityGrantVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledEquityGrantVariance item)
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

    public List<ScheduledEquityGrantVariance> List()
    {
        var outValue = new List<ScheduledEquityGrantVariance>();
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
public sealed class CachedScheduledEquityGrantVarianceStore(MemoryScheduledEquityGrantVarianceStore inner, ScheduledEquityGrantVarianceCache cache, int hits, int misses) : IScheduledEquityGrantVarianceStore
{
    public MemoryScheduledEquityGrantVarianceStore Inner { get; set; } = inner;
    public ScheduledEquityGrantVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledEquityGrantVariance? Find(string code)
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

    public void Save(ScheduledEquityGrantVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledEquityGrantVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled equity grant variances. Empty fields match all.</summary>
public sealed record ScheduledEquityGrantVarianceQuery(string Region, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledEquityGrantVariance item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledEquityGrantVariance> Apply(List<ScheduledEquityGrantVariance> items)
    {
        var matched = Collections.Filter(items, (ScheduledEquityGrantVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled equity grant variance store.</summary>
public static class ScheduledEquityGrantVarianceStore
{
    public static IScheduledEquityGrantVarianceStore NewMemoryScheduledEquityGrantVarianceStore()
    {
        return new MemoryScheduledEquityGrantVarianceStore(
            new Dictionary<string, ScheduledEquityGrantVariance>(),
            new List<string>());
    }

    public static IScheduledEquityGrantVarianceStore NewCachedScheduledEquityGrantVarianceStore(MemoryScheduledEquityGrantVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledEquityGrantVarianceStore(
            inner,
            ScheduledEquityGrantVarianceCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledEquityGrantVariance> FindScheduledEquityGrantVariances(IScheduledEquityGrantVarianceStore store, ScheduledEquityGrantVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
