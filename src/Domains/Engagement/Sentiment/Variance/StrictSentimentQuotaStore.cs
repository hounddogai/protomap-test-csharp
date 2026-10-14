namespace Acme.Workforce.Engagement.Sentiment.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict sentiment quotas by code.</summary>
public interface IStrictSentimentQuotaStore
{
    StrictSentimentQuota? Find(string code);
    void Save(StrictSentimentQuota item);
    bool Remove(string code);
    List<StrictSentimentQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictSentimentQuotaStore(Dictionary<string, StrictSentimentQuota> items, List<string> order) : IStrictSentimentQuotaStore
{
    public Dictionary<string, StrictSentimentQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictSentimentQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictSentimentQuota item)
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

    public List<StrictSentimentQuota> List()
    {
        var outValue = new List<StrictSentimentQuota>();
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
public sealed class CachedStrictSentimentQuotaStore(MemoryStrictSentimentQuotaStore inner, StrictSentimentQuotaCache cache, int hits, int misses) : IStrictSentimentQuotaStore
{
    public MemoryStrictSentimentQuotaStore Inner { get; set; } = inner;
    public StrictSentimentQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictSentimentQuota? Find(string code)
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

    public void Save(StrictSentimentQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictSentimentQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict sentiment quotas. Empty fields match all.</summary>
public sealed record StrictSentimentQuotaQuery(string Region, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(StrictSentimentQuota item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictSentimentQuota> Apply(List<StrictSentimentQuota> items)
    {
        var matched = Collections.Filter(items, (StrictSentimentQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict sentiment quota store.</summary>
public static class StrictSentimentQuotaStore
{
    public static IStrictSentimentQuotaStore NewMemoryStrictSentimentQuotaStore()
    {
        return new MemoryStrictSentimentQuotaStore(
            new Dictionary<string, StrictSentimentQuota>(),
            new List<string>());
    }

    public static IStrictSentimentQuotaStore NewCachedStrictSentimentQuotaStore(MemoryStrictSentimentQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedStrictSentimentQuotaStore(inner, StrictSentimentQuotaCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<StrictSentimentQuota> FindStrictSentimentQuotas(IStrictSentimentQuotaStore store, StrictSentimentQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
