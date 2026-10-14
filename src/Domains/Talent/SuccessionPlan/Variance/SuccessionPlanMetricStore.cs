namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan metrics by code.</summary>
public interface ISuccessionPlanMetricStore
{
    SuccessionPlanMetric? Find(string code);
    void Save(SuccessionPlanMetric item);
    bool Remove(string code);
    List<SuccessionPlanMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanMetricStore(Dictionary<string, SuccessionPlanMetric> items, List<string> order) : ISuccessionPlanMetricStore
{
    public Dictionary<string, SuccessionPlanMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanMetric item)
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

    public List<SuccessionPlanMetric> List()
    {
        var outValue = new List<SuccessionPlanMetric>();
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
public sealed class CachedSuccessionPlanMetricStore(MemorySuccessionPlanMetricStore inner, SuccessionPlanMetricCache cache, int hits, int misses) : ISuccessionPlanMetricStore
{
    public MemorySuccessionPlanMetricStore Inner { get; set; } = inner;
    public SuccessionPlanMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanMetric? Find(string code)
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

    public void Save(SuccessionPlanMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan metrics. Empty fields match all.</summary>
public sealed record SuccessionPlanMetricQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanMetric item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanMetric> Apply(List<SuccessionPlanMetric> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan metric store.</summary>
public static class SuccessionPlanMetricStore
{
    public static ISuccessionPlanMetricStore NewMemorySuccessionPlanMetricStore()
    {
        return new MemorySuccessionPlanMetricStore(
            new Dictionary<string, SuccessionPlanMetric>(),
            new List<string>());
    }

    public static ISuccessionPlanMetricStore NewCachedSuccessionPlanMetricStore(MemorySuccessionPlanMetricStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanMetricStore(inner, SuccessionPlanMetricCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SuccessionPlanMetric> FindSuccessionPlanMetrics(ISuccessionPlanMetricStore store, SuccessionPlanMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
