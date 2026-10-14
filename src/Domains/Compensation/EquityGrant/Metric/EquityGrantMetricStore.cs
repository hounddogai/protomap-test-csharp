namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant metrics by code.</summary>
public interface IEquityGrantMetricStore
{
    EquityGrantMetric? Find(string code);
    void Save(EquityGrantMetric item);
    bool Remove(string code);
    List<EquityGrantMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantMetricStore(Dictionary<string, EquityGrantMetric> items, List<string> order) : IEquityGrantMetricStore
{
    public Dictionary<string, EquityGrantMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantMetric item)
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

    public List<EquityGrantMetric> List()
    {
        var outValue = new List<EquityGrantMetric>();
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
public sealed class CachedEquityGrantMetricStore(MemoryEquityGrantMetricStore inner, EquityGrantMetricCache cache, int hits, int misses) : IEquityGrantMetricStore
{
    public MemoryEquityGrantMetricStore Inner { get; set; } = inner;
    public EquityGrantMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantMetric? Find(string code)
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

    public void Save(EquityGrantMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant metrics. Empty fields match all.</summary>
public sealed record EquityGrantMetricQuery(string Department, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantMetric item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantMetric> Apply(List<EquityGrantMetric> items)
    {
        var matched = Collections.Filter(items, (EquityGrantMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant metric store.</summary>
public static class EquityGrantMetricStore
{
    public static IEquityGrantMetricStore NewMemoryEquityGrantMetricStore()
    {
        return new MemoryEquityGrantMetricStore(new Dictionary<string, EquityGrantMetric>(), new List<string>());
    }

    public static IEquityGrantMetricStore NewCachedEquityGrantMetricStore(MemoryEquityGrantMetricStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantMetricStore(inner, EquityGrantMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<EquityGrantMetric> FindEquityGrantMetrics(IEquityGrantMetricStore store, EquityGrantMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
