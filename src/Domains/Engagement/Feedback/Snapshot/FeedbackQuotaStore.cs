namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback quotas by code.</summary>
public interface IFeedbackQuotaStore
{
    FeedbackQuota? Find(string code);
    void Save(FeedbackQuota item);
    bool Remove(string code);
    List<FeedbackQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackQuotaStore(Dictionary<string, FeedbackQuota> items, List<string> order) : IFeedbackQuotaStore
{
    public Dictionary<string, FeedbackQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackQuota item)
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

    public List<FeedbackQuota> List()
    {
        var outValue = new List<FeedbackQuota>();
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
public sealed class CachedFeedbackQuotaStore(MemoryFeedbackQuotaStore inner, FeedbackQuotaCache cache, int hits, int misses) : IFeedbackQuotaStore
{
    public MemoryFeedbackQuotaStore Inner { get; set; } = inner;
    public FeedbackQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackQuota? Find(string code)
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

    public void Save(FeedbackQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback quotas. Empty fields match all.</summary>
public sealed record FeedbackQuotaQuery(string Region, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackQuota item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FeedbackQuota> Apply(List<FeedbackQuota> items)
    {
        var matched = Collections.Filter(items, (FeedbackQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback quota store.</summary>
public static class FeedbackQuotaStore
{
    public static IFeedbackQuotaStore NewMemoryFeedbackQuotaStore()
    {
        return new MemoryFeedbackQuotaStore(new Dictionary<string, FeedbackQuota>(), new List<string>());
    }

    public static IFeedbackQuotaStore NewCachedFeedbackQuotaStore(MemoryFeedbackQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackQuotaStore(inner, FeedbackQuotaCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<FeedbackQuota> FindFeedbackQuotas(IFeedbackQuotaStore store, FeedbackQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
