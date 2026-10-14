namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal recognition metrics by code.</summary>
public interface ISeasonalRecognitionMetricStore
{
    SeasonalRecognitionMetric? Find(string code);
    void Save(SeasonalRecognitionMetric item);
    bool Remove(string code);
    List<SeasonalRecognitionMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalRecognitionMetricStore(Dictionary<string, SeasonalRecognitionMetric> items, List<string> order) : ISeasonalRecognitionMetricStore
{
    public Dictionary<string, SeasonalRecognitionMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalRecognitionMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalRecognitionMetric item)
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

    public List<SeasonalRecognitionMetric> List()
    {
        var outValue = new List<SeasonalRecognitionMetric>();
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
public sealed class CachedSeasonalRecognitionMetricStore(MemorySeasonalRecognitionMetricStore inner, SeasonalRecognitionMetricCache cache, int hits, int misses) : ISeasonalRecognitionMetricStore
{
    public MemorySeasonalRecognitionMetricStore Inner { get; set; } = inner;
    public SeasonalRecognitionMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalRecognitionMetric? Find(string code)
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

    public void Save(SeasonalRecognitionMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalRecognitionMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal recognition metrics. Empty fields match all.</summary>
public sealed record SeasonalRecognitionMetricQuery(string Title, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalRecognitionMetric item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalRecognitionMetric> Apply(List<SeasonalRecognitionMetric> items)
    {
        var matched = Collections.Filter(items, (SeasonalRecognitionMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal recognition metric store.</summary>
public static class SeasonalRecognitionMetricStore
{
    public static ISeasonalRecognitionMetricStore NewMemorySeasonalRecognitionMetricStore()
    {
        return new MemorySeasonalRecognitionMetricStore(
            new Dictionary<string, SeasonalRecognitionMetric>(),
            new List<string>());
    }

    public static ISeasonalRecognitionMetricStore NewCachedSeasonalRecognitionMetricStore(MemorySeasonalRecognitionMetricStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalRecognitionMetricStore(
            inner,
            SeasonalRecognitionMetricCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalRecognitionMetric> FindSeasonalRecognitionMetrics(ISeasonalRecognitionMetricStore store, SeasonalRecognitionMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
