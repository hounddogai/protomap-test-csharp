namespace Acme.Workforce.Engagement.Recognition.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled recognition metrics by code.</summary>
public interface IScheduledRecognitionMetricStore
{
    ScheduledRecognitionMetric? Find(string code);
    void Save(ScheduledRecognitionMetric item);
    bool Remove(string code);
    List<ScheduledRecognitionMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledRecognitionMetricStore(Dictionary<string, ScheduledRecognitionMetric> items, List<string> order) : IScheduledRecognitionMetricStore
{
    public Dictionary<string, ScheduledRecognitionMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledRecognitionMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledRecognitionMetric item)
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

    public List<ScheduledRecognitionMetric> List()
    {
        var outValue = new List<ScheduledRecognitionMetric>();
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
public sealed class CachedScheduledRecognitionMetricStore(MemoryScheduledRecognitionMetricStore inner, ScheduledRecognitionMetricCache cache, int hits, int misses) : IScheduledRecognitionMetricStore
{
    public MemoryScheduledRecognitionMetricStore Inner { get; set; } = inner;
    public ScheduledRecognitionMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledRecognitionMetric? Find(string code)
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

    public void Save(ScheduledRecognitionMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledRecognitionMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled recognition metrics. Empty fields match all.</summary>
public sealed record ScheduledRecognitionMetricQuery(string Name, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledRecognitionMetric item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<ScheduledRecognitionMetric> Apply(List<ScheduledRecognitionMetric> items)
    {
        var matched = Collections.Filter(items, (ScheduledRecognitionMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled recognition metric store.</summary>
public static class ScheduledRecognitionMetricStore
{
    public static IScheduledRecognitionMetricStore NewMemoryScheduledRecognitionMetricStore()
    {
        return new MemoryScheduledRecognitionMetricStore(
            new Dictionary<string, ScheduledRecognitionMetric>(),
            new List<string>());
    }

    public static IScheduledRecognitionMetricStore NewCachedScheduledRecognitionMetricStore(MemoryScheduledRecognitionMetricStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledRecognitionMetricStore(
            inner,
            ScheduledRecognitionMetricCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledRecognitionMetric> FindScheduledRecognitionMetrics(IScheduledRecognitionMetricStore store, ScheduledRecognitionMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
