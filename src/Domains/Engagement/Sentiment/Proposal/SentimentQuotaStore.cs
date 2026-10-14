namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment quotas by code.</summary>
public interface ISentimentQuotaStore
{
    SentimentQuota? Find(string code);
    void Save(SentimentQuota item);
    bool Remove(string code);
    List<SentimentQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentQuotaStore(Dictionary<string, SentimentQuota> items, List<string> order) : ISentimentQuotaStore
{
    public Dictionary<string, SentimentQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentQuota item)
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

    public List<SentimentQuota> List()
    {
        var outValue = new List<SentimentQuota>();
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
public sealed class CachedSentimentQuotaStore(MemorySentimentQuotaStore inner, SentimentQuotaCache cache, int hits, int misses) : ISentimentQuotaStore
{
    public MemorySentimentQuotaStore Inner { get; set; } = inner;
    public SentimentQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentQuota? Find(string code)
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

    public void Save(SentimentQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment quotas. Empty fields match all.</summary>
public sealed record SentimentQuotaQuery(string Unit, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentQuota item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<SentimentQuota> Apply(List<SentimentQuota> items)
    {
        var matched = Collections.Filter(items, (SentimentQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment quota store.</summary>
public static class SentimentQuotaStore
{
    public static ISentimentQuotaStore NewMemorySentimentQuotaStore()
    {
        return new MemorySentimentQuotaStore(new Dictionary<string, SentimentQuota>(), new List<string>());
    }

    public static ISentimentQuotaStore NewCachedSentimentQuotaStore(MemorySentimentQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentQuotaStore(inner, SentimentQuotaCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SentimentQuota> FindSentimentQuotas(ISentimentQuotaStore store, SentimentQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
