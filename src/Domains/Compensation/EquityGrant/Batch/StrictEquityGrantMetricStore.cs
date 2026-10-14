namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict equity grant metrics by code.</summary>
public interface IStrictEquityGrantMetricStore
{
    StrictEquityGrantMetric? Find(string code);
    void Save(StrictEquityGrantMetric item);
    bool Remove(string code);
    List<StrictEquityGrantMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictEquityGrantMetricStore(Dictionary<string, StrictEquityGrantMetric> items, List<string> order) : IStrictEquityGrantMetricStore
{
    public Dictionary<string, StrictEquityGrantMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictEquityGrantMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictEquityGrantMetric item)
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

    public List<StrictEquityGrantMetric> List()
    {
        var outValue = new List<StrictEquityGrantMetric>();
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
public sealed class CachedStrictEquityGrantMetricStore(MemoryStrictEquityGrantMetricStore inner, StrictEquityGrantMetricCache cache, int hits, int misses) : IStrictEquityGrantMetricStore
{
    public MemoryStrictEquityGrantMetricStore Inner { get; set; } = inner;
    public StrictEquityGrantMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictEquityGrantMetric? Find(string code)
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

    public void Save(StrictEquityGrantMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictEquityGrantMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict equity grant metrics. Empty fields match all.</summary>
public sealed record StrictEquityGrantMetricQuery(string Unit, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(StrictEquityGrantMetric item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictEquityGrantMetric> Apply(List<StrictEquityGrantMetric> items)
    {
        var matched = Collections.Filter(items, (StrictEquityGrantMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict equity grant metric store.</summary>
public static class StrictEquityGrantMetricStore
{
    public static IStrictEquityGrantMetricStore NewMemoryStrictEquityGrantMetricStore()
    {
        return new MemoryStrictEquityGrantMetricStore(
            new Dictionary<string, StrictEquityGrantMetric>(),
            new List<string>());
    }

    public static IStrictEquityGrantMetricStore NewCachedStrictEquityGrantMetricStore(MemoryStrictEquityGrantMetricStore inner, int ttl, IClock clock)
    {
        return new CachedStrictEquityGrantMetricStore(
            inner,
            StrictEquityGrantMetricCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<StrictEquityGrantMetric> FindStrictEquityGrantMetrics(IStrictEquityGrantMetricStore store, StrictEquityGrantMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
