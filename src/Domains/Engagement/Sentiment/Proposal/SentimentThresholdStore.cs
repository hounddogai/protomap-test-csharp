namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment thresholds by code.</summary>
public interface ISentimentThresholdStore
{
    SentimentThreshold? Find(string code);
    void Save(SentimentThreshold item);
    bool Remove(string code);
    List<SentimentThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentThresholdStore(Dictionary<string, SentimentThreshold> items, List<string> order) : ISentimentThresholdStore
{
    public Dictionary<string, SentimentThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentThreshold item)
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

    public List<SentimentThreshold> List()
    {
        var outValue = new List<SentimentThreshold>();
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
public sealed class CachedSentimentThresholdStore(MemorySentimentThresholdStore inner, SentimentThresholdCache cache, int hits, int misses) : ISentimentThresholdStore
{
    public MemorySentimentThresholdStore Inner { get; set; } = inner;
    public SentimentThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentThreshold? Find(string code)
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

    public void Save(SentimentThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment thresholds. Empty fields match all.</summary>
public sealed record SentimentThresholdQuery(string Title, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentThreshold item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentThreshold> Apply(List<SentimentThreshold> items)
    {
        var matched = Collections.Filter(items, (SentimentThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment threshold store.</summary>
public static class SentimentThresholdStore
{
    public static ISentimentThresholdStore NewMemorySentimentThresholdStore()
    {
        return new MemorySentimentThresholdStore(new Dictionary<string, SentimentThreshold>(), new List<string>());
    }

    public static ISentimentThresholdStore NewCachedSentimentThresholdStore(MemorySentimentThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentThresholdStore(inner, SentimentThresholdCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SentimentThreshold> FindSentimentThresholds(ISentimentThresholdStore store, SentimentThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
