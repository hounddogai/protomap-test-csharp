namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment digests by code.</summary>
public interface ISentimentDigestStore
{
    SentimentDigest? Find(string code);
    void Save(SentimentDigest item);
    bool Remove(string code);
    List<SentimentDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentDigestStore(Dictionary<string, SentimentDigest> items, List<string> order) : ISentimentDigestStore
{
    public Dictionary<string, SentimentDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentDigest item)
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

    public List<SentimentDigest> List()
    {
        var outValue = new List<SentimentDigest>();
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
public sealed class CachedSentimentDigestStore(MemorySentimentDigestStore inner, SentimentDigestCache cache, int hits, int misses) : ISentimentDigestStore
{
    public MemorySentimentDigestStore Inner { get; set; } = inner;
    public SentimentDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentDigest? Find(string code)
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

    public void Save(SentimentDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment digests. Empty fields match all.</summary>
public sealed record SentimentDigestQuery(string Title, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentDigest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentDigest> Apply(List<SentimentDigest> items)
    {
        var matched = Collections.Filter(items, (SentimentDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment digest store.</summary>
public static class SentimentDigestStore
{
    public static ISentimentDigestStore NewMemorySentimentDigestStore()
    {
        return new MemorySentimentDigestStore(new Dictionary<string, SentimentDigest>(), new List<string>());
    }

    public static ISentimentDigestStore NewCachedSentimentDigestStore(MemorySentimentDigestStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentDigestStore(inner, SentimentDigestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SentimentDigest> FindSentimentDigests(ISentimentDigestStore store, SentimentDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
