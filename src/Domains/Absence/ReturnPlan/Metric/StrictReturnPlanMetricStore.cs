namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict return plan metrics by code.</summary>
public interface IStrictReturnPlanMetricStore
{
    StrictReturnPlanMetric? Find(string code);
    void Save(StrictReturnPlanMetric item);
    bool Remove(string code);
    List<StrictReturnPlanMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictReturnPlanMetricStore(Dictionary<string, StrictReturnPlanMetric> items, List<string> order) : IStrictReturnPlanMetricStore
{
    public Dictionary<string, StrictReturnPlanMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictReturnPlanMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictReturnPlanMetric item)
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

    public List<StrictReturnPlanMetric> List()
    {
        var outValue = new List<StrictReturnPlanMetric>();
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
public sealed class CachedStrictReturnPlanMetricStore(MemoryStrictReturnPlanMetricStore inner, StrictReturnPlanMetricCache cache, int hits, int misses) : IStrictReturnPlanMetricStore
{
    public MemoryStrictReturnPlanMetricStore Inner { get; set; } = inner;
    public StrictReturnPlanMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictReturnPlanMetric? Find(string code)
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

    public void Save(StrictReturnPlanMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictReturnPlanMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict return plan metrics. Empty fields match all.</summary>
public sealed record StrictReturnPlanMetricQuery(string Category, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(StrictReturnPlanMetric item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictReturnPlanMetric> Apply(List<StrictReturnPlanMetric> items)
    {
        var matched = Collections.Filter(items, (StrictReturnPlanMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict return plan metric store.</summary>
public static class StrictReturnPlanMetricStore
{
    public static IStrictReturnPlanMetricStore NewMemoryStrictReturnPlanMetricStore()
    {
        return new MemoryStrictReturnPlanMetricStore(
            new Dictionary<string, StrictReturnPlanMetric>(),
            new List<string>());
    }

    public static IStrictReturnPlanMetricStore NewCachedStrictReturnPlanMetricStore(MemoryStrictReturnPlanMetricStore inner, int ttl, IClock clock)
    {
        return new CachedStrictReturnPlanMetricStore(
            inner,
            StrictReturnPlanMetricCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<StrictReturnPlanMetric> FindStrictReturnPlanMetrics(IStrictReturnPlanMetricStore store, StrictReturnPlanMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
