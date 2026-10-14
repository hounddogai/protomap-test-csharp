namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment drafts by code.</summary>
public interface ISentimentDraftStore
{
    SentimentDraft? Find(string code);
    void Save(SentimentDraft item);
    bool Remove(string code);
    List<SentimentDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentDraftStore(Dictionary<string, SentimentDraft> items, List<string> order) : ISentimentDraftStore
{
    public Dictionary<string, SentimentDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentDraft item)
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

    public List<SentimentDraft> List()
    {
        var outValue = new List<SentimentDraft>();
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
public sealed class CachedSentimentDraftStore(MemorySentimentDraftStore inner, SentimentDraftCache cache, int hits, int misses) : ISentimentDraftStore
{
    public MemorySentimentDraftStore Inner { get; set; } = inner;
    public SentimentDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentDraft? Find(string code)
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

    public void Save(SentimentDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment drafts. Empty fields match all.</summary>
public sealed record SentimentDraftQuery(string Title, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentDraft item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentDraft> Apply(List<SentimentDraft> items)
    {
        var matched = Collections.Filter(items, (SentimentDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment draft store.</summary>
public static class SentimentDraftStore
{
    public static ISentimentDraftStore NewMemorySentimentDraftStore()
    {
        return new MemorySentimentDraftStore(new Dictionary<string, SentimentDraft>(), new List<string>());
    }

    public static ISentimentDraftStore NewCachedSentimentDraftStore(MemorySentimentDraftStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentDraftStore(inner, SentimentDraftCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SentimentDraft> FindSentimentDrafts(ISentimentDraftStore store, SentimentDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
