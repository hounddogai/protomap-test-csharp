namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment tiers by code.</summary>
public interface ISentimentTierStore
{
    SentimentTier? Find(string code);
    void Save(SentimentTier item);
    bool Remove(string code);
    List<SentimentTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentTierStore(Dictionary<string, SentimentTier> items, List<string> order) : ISentimentTierStore
{
    public Dictionary<string, SentimentTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentTier item)
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

    public List<SentimentTier> List()
    {
        var outValue = new List<SentimentTier>();
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
public sealed class CachedSentimentTierStore(MemorySentimentTierStore inner, SentimentTierCache cache, int hits, int misses) : ISentimentTierStore
{
    public MemorySentimentTierStore Inner { get; set; } = inner;
    public SentimentTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentTier? Find(string code)
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

    public void Save(SentimentTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment tiers. Empty fields match all.</summary>
public sealed record SentimentTierQuery(string Team, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentTier item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<SentimentTier> Apply(List<SentimentTier> items)
    {
        var matched = Collections.Filter(items, (SentimentTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment tier store.</summary>
public static class SentimentTierStore
{
    public static ISentimentTierStore NewMemorySentimentTierStore()
    {
        return new MemorySentimentTierStore(new Dictionary<string, SentimentTier>(), new List<string>());
    }

    public static ISentimentTierStore NewCachedSentimentTierStore(MemorySentimentTierStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentTierStore(inner, SentimentTierCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SentimentTier> FindSentimentTiers(ISentimentTierStore store, SentimentTierQuery query)
    {
        return query.Apply(store.List());
    }
}
