namespace Acme.Workforce.Absence.ReturnPlan.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan metrics by code.</summary>
public interface IReturnPlanMetricStore
{
    ReturnPlanMetric? Find(string code);
    void Save(ReturnPlanMetric item);
    bool Remove(string code);
    List<ReturnPlanMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanMetricStore(Dictionary<string, ReturnPlanMetric> items, List<string> order) : IReturnPlanMetricStore
{
    public Dictionary<string, ReturnPlanMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanMetric item)
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

    public List<ReturnPlanMetric> List()
    {
        var outValue = new List<ReturnPlanMetric>();
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
public sealed class CachedReturnPlanMetricStore(MemoryReturnPlanMetricStore inner, ReturnPlanMetricCache cache, int hits, int misses) : IReturnPlanMetricStore
{
    public MemoryReturnPlanMetricStore Inner { get; set; } = inner;
    public ReturnPlanMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanMetric? Find(string code)
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

    public void Save(ReturnPlanMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan metrics. Empty fields match all.</summary>
public sealed record ReturnPlanMetricQuery(string Segment, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanMetric item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReturnPlanMetric> Apply(List<ReturnPlanMetric> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan metric store.</summary>
public static class ReturnPlanMetricStore
{
    public static IReturnPlanMetricStore NewMemoryReturnPlanMetricStore()
    {
        return new MemoryReturnPlanMetricStore(new Dictionary<string, ReturnPlanMetric>(), new List<string>());
    }

    public static IReturnPlanMetricStore NewCachedReturnPlanMetricStore(MemoryReturnPlanMetricStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanMetricStore(inner, ReturnPlanMetricCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanMetric> FindReturnPlanMetrics(IReturnPlanMetricStore store, ReturnPlanMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
