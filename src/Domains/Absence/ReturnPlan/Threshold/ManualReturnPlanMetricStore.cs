namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual return plan metrics by code.</summary>
public interface IManualReturnPlanMetricStore
{
    ManualReturnPlanMetric? Find(string code);
    void Save(ManualReturnPlanMetric item);
    bool Remove(string code);
    List<ManualReturnPlanMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualReturnPlanMetricStore(Dictionary<string, ManualReturnPlanMetric> items, List<string> order) : IManualReturnPlanMetricStore
{
    public Dictionary<string, ManualReturnPlanMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualReturnPlanMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualReturnPlanMetric item)
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

    public List<ManualReturnPlanMetric> List()
    {
        var outValue = new List<ManualReturnPlanMetric>();
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
public sealed class CachedManualReturnPlanMetricStore(MemoryManualReturnPlanMetricStore inner, ManualReturnPlanMetricCache cache, int hits, int misses) : IManualReturnPlanMetricStore
{
    public MemoryManualReturnPlanMetricStore Inner { get; set; } = inner;
    public ManualReturnPlanMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualReturnPlanMetric? Find(string code)
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

    public void Save(ManualReturnPlanMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualReturnPlanMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual return plan metrics. Empty fields match all.</summary>
public sealed record ManualReturnPlanMetricQuery(string Segment, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualReturnPlanMetric item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualReturnPlanMetric> Apply(List<ManualReturnPlanMetric> items)
    {
        var matched = Collections.Filter(items, (ManualReturnPlanMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual return plan metric store.</summary>
public static class ManualReturnPlanMetricStore
{
    public static IManualReturnPlanMetricStore NewMemoryManualReturnPlanMetricStore()
    {
        return new MemoryManualReturnPlanMetricStore(
            new Dictionary<string, ManualReturnPlanMetric>(),
            new List<string>());
    }

    public static IManualReturnPlanMetricStore NewCachedManualReturnPlanMetricStore(MemoryManualReturnPlanMetricStore inner, int ttl, IClock clock)
    {
        return new CachedManualReturnPlanMetricStore(
            inner,
            ManualReturnPlanMetricCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ManualReturnPlanMetric> FindManualReturnPlanMetrics(IManualReturnPlanMetricStore store, ManualReturnPlanMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
