namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient feedback metrics by code.</summary>
public interface ILenientFeedbackMetricStore
{
    LenientFeedbackMetric? Find(string code);
    void Save(LenientFeedbackMetric item);
    bool Remove(string code);
    List<LenientFeedbackMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientFeedbackMetricStore(Dictionary<string, LenientFeedbackMetric> items, List<string> order) : ILenientFeedbackMetricStore
{
    public Dictionary<string, LenientFeedbackMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientFeedbackMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientFeedbackMetric item)
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

    public List<LenientFeedbackMetric> List()
    {
        var outValue = new List<LenientFeedbackMetric>();
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
public sealed class CachedLenientFeedbackMetricStore(MemoryLenientFeedbackMetricStore inner, LenientFeedbackMetricCache cache, int hits, int misses) : ILenientFeedbackMetricStore
{
    public MemoryLenientFeedbackMetricStore Inner { get; set; } = inner;
    public LenientFeedbackMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientFeedbackMetric? Find(string code)
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

    public void Save(LenientFeedbackMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientFeedbackMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient feedback metrics. Empty fields match all.</summary>
public sealed record LenientFeedbackMetricQuery(string Segment, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(LenientFeedbackMetric item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<LenientFeedbackMetric> Apply(List<LenientFeedbackMetric> items)
    {
        var matched = Collections.Filter(items, (LenientFeedbackMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient feedback metric store.</summary>
public static class LenientFeedbackMetricStore
{
    public static ILenientFeedbackMetricStore NewMemoryLenientFeedbackMetricStore()
    {
        return new MemoryLenientFeedbackMetricStore(
            new Dictionary<string, LenientFeedbackMetric>(),
            new List<string>());
    }

    public static ILenientFeedbackMetricStore NewCachedLenientFeedbackMetricStore(MemoryLenientFeedbackMetricStore inner, int ttl, IClock clock)
    {
        return new CachedLenientFeedbackMetricStore(
            inner,
            LenientFeedbackMetricCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientFeedbackMetric> FindLenientFeedbackMetrics(ILenientFeedbackMetricStore store, LenientFeedbackMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
