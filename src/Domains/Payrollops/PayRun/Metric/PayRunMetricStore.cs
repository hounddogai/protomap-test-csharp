namespace Acme.Workforce.Payrollops.PayRun.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run metrics by code.</summary>
public interface IPayRunMetricStore
{
    PayRunMetric? Find(string code);
    void Save(PayRunMetric item);
    bool Remove(string code);
    List<PayRunMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunMetricStore(Dictionary<string, PayRunMetric> items, List<string> order) : IPayRunMetricStore
{
    public Dictionary<string, PayRunMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunMetric item)
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

    public List<PayRunMetric> List()
    {
        var outValue = new List<PayRunMetric>();
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
public sealed class CachedPayRunMetricStore(MemoryPayRunMetricStore inner, PayRunMetricCache cache, int hits, int misses) : IPayRunMetricStore
{
    public MemoryPayRunMetricStore Inner { get; set; } = inner;
    public PayRunMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunMetric? Find(string code)
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

    public void Save(PayRunMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run metrics. Empty fields match all.</summary>
public sealed record PayRunMetricQuery(string Channel, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunMetric item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunMetric> Apply(List<PayRunMetric> items)
    {
        var matched = Collections.Filter(items, (PayRunMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run metric store.</summary>
public static class PayRunMetricStore
{
    public static IPayRunMetricStore NewMemoryPayRunMetricStore()
    {
        return new MemoryPayRunMetricStore(new Dictionary<string, PayRunMetric>(), new List<string>());
    }

    public static IPayRunMetricStore NewCachedPayRunMetricStore(MemoryPayRunMetricStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunMetricStore(inner, PayRunMetricCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayRunMetric> FindPayRunMetrics(IPayRunMetricStore store, PayRunMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
