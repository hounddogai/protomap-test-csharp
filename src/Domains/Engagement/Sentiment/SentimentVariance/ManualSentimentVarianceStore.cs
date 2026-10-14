namespace Acme.Workforce.Engagement.Sentiment.SentimentVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual sentiment variances by code.</summary>
public interface IManualSentimentVarianceStore
{
    ManualSentimentVariance? Find(string code);
    void Save(ManualSentimentVariance item);
    bool Remove(string code);
    List<ManualSentimentVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualSentimentVarianceStore(Dictionary<string, ManualSentimentVariance> items, List<string> order) : IManualSentimentVarianceStore
{
    public Dictionary<string, ManualSentimentVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualSentimentVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualSentimentVariance item)
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

    public List<ManualSentimentVariance> List()
    {
        var outValue = new List<ManualSentimentVariance>();
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
public sealed class CachedManualSentimentVarianceStore(MemoryManualSentimentVarianceStore inner, ManualSentimentVarianceCache cache, int hits, int misses) : IManualSentimentVarianceStore
{
    public MemoryManualSentimentVarianceStore Inner { get; set; } = inner;
    public ManualSentimentVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualSentimentVariance? Find(string code)
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

    public void Save(ManualSentimentVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualSentimentVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual sentiment variances. Empty fields match all.</summary>
public sealed record ManualSentimentVarianceQuery(string Department, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(ManualSentimentVariance item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<ManualSentimentVariance> Apply(List<ManualSentimentVariance> items)
    {
        var matched = Collections.Filter(items, (ManualSentimentVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual sentiment variance store.</summary>
public static class ManualSentimentVarianceStore
{
    public static IManualSentimentVarianceStore NewMemoryManualSentimentVarianceStore()
    {
        return new MemoryManualSentimentVarianceStore(
            new Dictionary<string, ManualSentimentVariance>(),
            new List<string>());
    }

    public static IManualSentimentVarianceStore NewCachedManualSentimentVarianceStore(MemoryManualSentimentVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedManualSentimentVarianceStore(
            inner,
            ManualSentimentVarianceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ManualSentimentVariance> FindManualSentimentVariances(IManualSentimentVarianceStore store, ManualSentimentVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
