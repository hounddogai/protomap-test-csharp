namespace Acme.Workforce.Engagement.Recognition.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition metrics by code.</summary>
public interface IRecognitionMetricStore
{
    RecognitionMetric? Find(string code);
    void Save(RecognitionMetric item);
    bool Remove(string code);
    List<RecognitionMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionMetricStore(Dictionary<string, RecognitionMetric> items, List<string> order) : IRecognitionMetricStore
{
    public Dictionary<string, RecognitionMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionMetric item)
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

    public List<RecognitionMetric> List()
    {
        var outValue = new List<RecognitionMetric>();
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
public sealed class CachedRecognitionMetricStore(MemoryRecognitionMetricStore inner, RecognitionMetricCache cache, int hits, int misses) : IRecognitionMetricStore
{
    public MemoryRecognitionMetricStore Inner { get; set; } = inner;
    public RecognitionMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionMetric? Find(string code)
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

    public void Save(RecognitionMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition metrics. Empty fields match all.</summary>
public sealed record RecognitionMetricQuery(string Locale, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionMetric item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionMetric> Apply(List<RecognitionMetric> items)
    {
        var matched = Collections.Filter(items, (RecognitionMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition metric store.</summary>
public static class RecognitionMetricStore
{
    public static IRecognitionMetricStore NewMemoryRecognitionMetricStore()
    {
        return new MemoryRecognitionMetricStore(new Dictionary<string, RecognitionMetric>(), new List<string>());
    }

    public static IRecognitionMetricStore NewCachedRecognitionMetricStore(MemoryRecognitionMetricStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionMetricStore(inner, RecognitionMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RecognitionMetric> FindRecognitionMetrics(IRecognitionMetricStore store, RecognitionMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
