namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled feedback metrics by code.</summary>
public interface IScheduledFeedbackMetricStore
{
    ScheduledFeedbackMetric? Find(string code);
    void Save(ScheduledFeedbackMetric item);
    bool Remove(string code);
    List<ScheduledFeedbackMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledFeedbackMetricStore(Dictionary<string, ScheduledFeedbackMetric> items, List<string> order) : IScheduledFeedbackMetricStore
{
    public Dictionary<string, ScheduledFeedbackMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledFeedbackMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledFeedbackMetric item)
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

    public List<ScheduledFeedbackMetric> List()
    {
        var outValue = new List<ScheduledFeedbackMetric>();
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
public sealed class CachedScheduledFeedbackMetricStore(MemoryScheduledFeedbackMetricStore inner, ScheduledFeedbackMetricCache cache, int hits, int misses) : IScheduledFeedbackMetricStore
{
    public MemoryScheduledFeedbackMetricStore Inner { get; set; } = inner;
    public ScheduledFeedbackMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledFeedbackMetric? Find(string code)
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

    public void Save(ScheduledFeedbackMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledFeedbackMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled feedback metrics. Empty fields match all.</summary>
public sealed record ScheduledFeedbackMetricQuery(string Source, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledFeedbackMetric item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledFeedbackMetric> Apply(List<ScheduledFeedbackMetric> items)
    {
        var matched = Collections.Filter(items, (ScheduledFeedbackMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled feedback metric store.</summary>
public static class ScheduledFeedbackMetricStore
{
    public static IScheduledFeedbackMetricStore NewMemoryScheduledFeedbackMetricStore()
    {
        return new MemoryScheduledFeedbackMetricStore(
            new Dictionary<string, ScheduledFeedbackMetric>(),
            new List<string>());
    }

    public static IScheduledFeedbackMetricStore NewCachedScheduledFeedbackMetricStore(MemoryScheduledFeedbackMetricStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledFeedbackMetricStore(
            inner,
            ScheduledFeedbackMetricCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledFeedbackMetric> FindScheduledFeedbackMetrics(IScheduledFeedbackMetricStore store, ScheduledFeedbackMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
