namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment requests by code.</summary>
public interface ISentimentRequestStore
{
    SentimentRequest? Find(string code);
    void Save(SentimentRequest item);
    bool Remove(string code);
    List<SentimentRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentRequestStore(Dictionary<string, SentimentRequest> items, List<string> order) : ISentimentRequestStore
{
    public Dictionary<string, SentimentRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentRequest item)
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

    public List<SentimentRequest> List()
    {
        var outValue = new List<SentimentRequest>();
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
public sealed class CachedSentimentRequestStore(MemorySentimentRequestStore inner, SentimentRequestCache cache, int hits, int misses) : ISentimentRequestStore
{
    public MemorySentimentRequestStore Inner { get; set; } = inner;
    public SentimentRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentRequest? Find(string code)
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

    public void Save(SentimentRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment requests. Empty fields match all.</summary>
public sealed record SentimentRequestQuery(string Source, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentRequest item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentRequest> Apply(List<SentimentRequest> items)
    {
        var matched = Collections.Filter(items, (SentimentRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment request store.</summary>
public static class SentimentRequestStore
{
    public static ISentimentRequestStore NewMemorySentimentRequestStore()
    {
        return new MemorySentimentRequestStore(new Dictionary<string, SentimentRequest>(), new List<string>());
    }

    public static ISentimentRequestStore NewCachedSentimentRequestStore(MemorySentimentRequestStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentRequestStore(inner, SentimentRequestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SentimentRequest> FindSentimentRequests(ISentimentRequestStore store, SentimentRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
