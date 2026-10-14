namespace Acme.Workforce.Engagement.Sentiment.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient sentiment thresholds by code.</summary>
public interface ILenientSentimentThresholdStore
{
    LenientSentimentThreshold? Find(string code);
    void Save(LenientSentimentThreshold item);
    bool Remove(string code);
    List<LenientSentimentThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientSentimentThresholdStore(Dictionary<string, LenientSentimentThreshold> items, List<string> order) : ILenientSentimentThresholdStore
{
    public Dictionary<string, LenientSentimentThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientSentimentThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientSentimentThreshold item)
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

    public List<LenientSentimentThreshold> List()
    {
        var outValue = new List<LenientSentimentThreshold>();
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
public sealed class CachedLenientSentimentThresholdStore(MemoryLenientSentimentThresholdStore inner, LenientSentimentThresholdCache cache, int hits, int misses) : ILenientSentimentThresholdStore
{
    public MemoryLenientSentimentThresholdStore Inner { get; set; } = inner;
    public LenientSentimentThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientSentimentThreshold? Find(string code)
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

    public void Save(LenientSentimentThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientSentimentThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient sentiment thresholds. Empty fields match all.</summary>
public sealed record LenientSentimentThresholdQuery(string Country, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LenientSentimentThreshold item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<LenientSentimentThreshold> Apply(List<LenientSentimentThreshold> items)
    {
        var matched = Collections.Filter(items, (LenientSentimentThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient sentiment threshold store.</summary>
public static class LenientSentimentThresholdStore
{
    public static ILenientSentimentThresholdStore NewMemoryLenientSentimentThresholdStore()
    {
        return new MemoryLenientSentimentThresholdStore(
            new Dictionary<string, LenientSentimentThreshold>(),
            new List<string>());
    }

    public static ILenientSentimentThresholdStore NewCachedLenientSentimentThresholdStore(MemoryLenientSentimentThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedLenientSentimentThresholdStore(
            inner,
            LenientSentimentThresholdCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LenientSentimentThreshold> FindLenientSentimentThresholds(ILenientSentimentThresholdStore store, LenientSentimentThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
