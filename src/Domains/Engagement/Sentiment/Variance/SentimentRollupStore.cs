namespace Acme.Workforce.Engagement.Sentiment.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment rollups by code.</summary>
public interface ISentimentRollupStore
{
    SentimentRollup? Find(string code);
    void Save(SentimentRollup item);
    bool Remove(string code);
    List<SentimentRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentRollupStore(Dictionary<string, SentimentRollup> items, List<string> order) : ISentimentRollupStore
{
    public Dictionary<string, SentimentRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentRollup item)
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

    public List<SentimentRollup> List()
    {
        var outValue = new List<SentimentRollup>();
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
public sealed class CachedSentimentRollupStore(MemorySentimentRollupStore inner, SentimentRollupCache cache, int hits, int misses) : ISentimentRollupStore
{
    public MemorySentimentRollupStore Inner { get; set; } = inner;
    public SentimentRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentRollup? Find(string code)
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

    public void Save(SentimentRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment rollups. Empty fields match all.</summary>
public sealed record SentimentRollupQuery(string Channel, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentRollup item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<SentimentRollup> Apply(List<SentimentRollup> items)
    {
        var matched = Collections.Filter(items, (SentimentRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment rollup store.</summary>
public static class SentimentRollupStore
{
    public static ISentimentRollupStore NewMemorySentimentRollupStore()
    {
        return new MemorySentimentRollupStore(new Dictionary<string, SentimentRollup>(), new List<string>());
    }

    public static ISentimentRollupStore NewCachedSentimentRollupStore(MemorySentimentRollupStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentRollupStore(inner, SentimentRollupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SentimentRollup> FindSentimentRollups(ISentimentRollupStore store, SentimentRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
