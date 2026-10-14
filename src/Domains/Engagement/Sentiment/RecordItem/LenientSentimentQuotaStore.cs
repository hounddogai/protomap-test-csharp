namespace Acme.Workforce.Engagement.Sentiment.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient sentiment quotas by code.</summary>
public interface ILenientSentimentQuotaStore
{
    LenientSentimentQuota? Find(string code);
    void Save(LenientSentimentQuota item);
    bool Remove(string code);
    List<LenientSentimentQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientSentimentQuotaStore(Dictionary<string, LenientSentimentQuota> items, List<string> order) : ILenientSentimentQuotaStore
{
    public Dictionary<string, LenientSentimentQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientSentimentQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientSentimentQuota item)
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

    public List<LenientSentimentQuota> List()
    {
        var outValue = new List<LenientSentimentQuota>();
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
public sealed class CachedLenientSentimentQuotaStore(MemoryLenientSentimentQuotaStore inner, LenientSentimentQuotaCache cache, int hits, int misses) : ILenientSentimentQuotaStore
{
    public MemoryLenientSentimentQuotaStore Inner { get; set; } = inner;
    public LenientSentimentQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientSentimentQuota? Find(string code)
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

    public void Save(LenientSentimentQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientSentimentQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient sentiment quotas. Empty fields match all.</summary>
public sealed record LenientSentimentQuotaQuery(string Title, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(LenientSentimentQuota item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientSentimentQuota> Apply(List<LenientSentimentQuota> items)
    {
        var matched = Collections.Filter(items, (LenientSentimentQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient sentiment quota store.</summary>
public static class LenientSentimentQuotaStore
{
    public static ILenientSentimentQuotaStore NewMemoryLenientSentimentQuotaStore()
    {
        return new MemoryLenientSentimentQuotaStore(
            new Dictionary<string, LenientSentimentQuota>(),
            new List<string>());
    }

    public static ILenientSentimentQuotaStore NewCachedLenientSentimentQuotaStore(MemoryLenientSentimentQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedLenientSentimentQuotaStore(inner, LenientSentimentQuotaCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LenientSentimentQuota> FindLenientSentimentQuotas(ILenientSentimentQuotaStore store, LenientSentimentQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
