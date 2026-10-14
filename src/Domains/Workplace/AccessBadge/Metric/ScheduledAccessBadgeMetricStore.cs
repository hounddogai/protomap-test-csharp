namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled access badge metrics by code.</summary>
public interface IScheduledAccessBadgeMetricStore
{
    ScheduledAccessBadgeMetric? Find(string code);
    void Save(ScheduledAccessBadgeMetric item);
    bool Remove(string code);
    List<ScheduledAccessBadgeMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledAccessBadgeMetricStore(Dictionary<string, ScheduledAccessBadgeMetric> items, List<string> order) : IScheduledAccessBadgeMetricStore
{
    public Dictionary<string, ScheduledAccessBadgeMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledAccessBadgeMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledAccessBadgeMetric item)
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

    public List<ScheduledAccessBadgeMetric> List()
    {
        var outValue = new List<ScheduledAccessBadgeMetric>();
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
public sealed class CachedScheduledAccessBadgeMetricStore(MemoryScheduledAccessBadgeMetricStore inner, ScheduledAccessBadgeMetricCache cache, int hits, int misses) : IScheduledAccessBadgeMetricStore
{
    public MemoryScheduledAccessBadgeMetricStore Inner { get; set; } = inner;
    public ScheduledAccessBadgeMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledAccessBadgeMetric? Find(string code)
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

    public void Save(ScheduledAccessBadgeMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledAccessBadgeMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled access badge metrics. Empty fields match all.</summary>
public sealed record ScheduledAccessBadgeMetricQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledAccessBadgeMetric item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledAccessBadgeMetric> Apply(List<ScheduledAccessBadgeMetric> items)
    {
        var matched = Collections.Filter(items, (ScheduledAccessBadgeMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled access badge metric store.</summary>
public static class ScheduledAccessBadgeMetricStore
{
    public static IScheduledAccessBadgeMetricStore NewMemoryScheduledAccessBadgeMetricStore()
    {
        return new MemoryScheduledAccessBadgeMetricStore(
            new Dictionary<string, ScheduledAccessBadgeMetric>(),
            new List<string>());
    }

    public static IScheduledAccessBadgeMetricStore NewCachedScheduledAccessBadgeMetricStore(MemoryScheduledAccessBadgeMetricStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledAccessBadgeMetricStore(
            inner,
            ScheduledAccessBadgeMetricCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledAccessBadgeMetric> FindScheduledAccessBadgeMetrics(IScheduledAccessBadgeMetricStore store, ScheduledAccessBadgeMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
