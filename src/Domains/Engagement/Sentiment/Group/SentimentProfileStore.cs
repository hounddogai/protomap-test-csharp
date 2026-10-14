namespace Acme.Workforce.Engagement.Sentiment.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment profiles by code.</summary>
public interface ISentimentProfileStore
{
    SentimentProfile? Find(string code);
    void Save(SentimentProfile item);
    bool Remove(string code);
    List<SentimentProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentProfileStore(Dictionary<string, SentimentProfile> items, List<string> order) : ISentimentProfileStore
{
    public Dictionary<string, SentimentProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentProfile item)
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

    public List<SentimentProfile> List()
    {
        var outValue = new List<SentimentProfile>();
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
public sealed class CachedSentimentProfileStore(MemorySentimentProfileStore inner, SentimentProfileCache cache, int hits, int misses) : ISentimentProfileStore
{
    public MemorySentimentProfileStore Inner { get; set; } = inner;
    public SentimentProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentProfile? Find(string code)
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

    public void Save(SentimentProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment profiles. Empty fields match all.</summary>
public sealed record SentimentProfileQuery(string Source, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentProfile item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentProfile> Apply(List<SentimentProfile> items)
    {
        var matched = Collections.Filter(items, (SentimentProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment profile store.</summary>
public static class SentimentProfileStore
{
    public static ISentimentProfileStore NewMemorySentimentProfileStore()
    {
        return new MemorySentimentProfileStore(new Dictionary<string, SentimentProfile>(), new List<string>());
    }

    public static ISentimentProfileStore NewCachedSentimentProfileStore(MemorySentimentProfileStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentProfileStore(inner, SentimentProfileCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SentimentProfile> FindSentimentProfiles(ISentimentProfileStore store, SentimentProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
