namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment cycles by code.</summary>
public interface ISentimentCycleStore
{
    SentimentCycle? Find(string code);
    void Save(SentimentCycle item);
    bool Remove(string code);
    List<SentimentCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentCycleStore(Dictionary<string, SentimentCycle> items, List<string> order) : ISentimentCycleStore
{
    public Dictionary<string, SentimentCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentCycle item)
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

    public List<SentimentCycle> List()
    {
        var outValue = new List<SentimentCycle>();
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
public sealed class CachedSentimentCycleStore(MemorySentimentCycleStore inner, SentimentCycleCache cache, int hits, int misses) : ISentimentCycleStore
{
    public MemorySentimentCycleStore Inner { get; set; } = inner;
    public SentimentCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentCycle? Find(string code)
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

    public void Save(SentimentCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment cycles. Empty fields match all.</summary>
public sealed record SentimentCycleQuery(string Name, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentCycle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentCycle> Apply(List<SentimentCycle> items)
    {
        var matched = Collections.Filter(items, (SentimentCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment cycle store.</summary>
public static class SentimentCycleStore
{
    public static ISentimentCycleStore NewMemorySentimentCycleStore()
    {
        return new MemorySentimentCycleStore(new Dictionary<string, SentimentCycle>(), new List<string>());
    }

    public static ISentimentCycleStore NewCachedSentimentCycleStore(MemorySentimentCycleStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentCycleStore(inner, SentimentCycleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SentimentCycle> FindSentimentCycles(ISentimentCycleStore store, SentimentCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
