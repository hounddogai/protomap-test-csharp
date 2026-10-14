namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict feedback quotas by code.</summary>
public interface IStrictFeedbackQuotaStore
{
    StrictFeedbackQuota? Find(string code);
    void Save(StrictFeedbackQuota item);
    bool Remove(string code);
    List<StrictFeedbackQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictFeedbackQuotaStore(Dictionary<string, StrictFeedbackQuota> items, List<string> order) : IStrictFeedbackQuotaStore
{
    public Dictionary<string, StrictFeedbackQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictFeedbackQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictFeedbackQuota item)
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

    public List<StrictFeedbackQuota> List()
    {
        var outValue = new List<StrictFeedbackQuota>();
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
public sealed class CachedStrictFeedbackQuotaStore(MemoryStrictFeedbackQuotaStore inner, StrictFeedbackQuotaCache cache, int hits, int misses) : IStrictFeedbackQuotaStore
{
    public MemoryStrictFeedbackQuotaStore Inner { get; set; } = inner;
    public StrictFeedbackQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictFeedbackQuota? Find(string code)
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

    public void Save(StrictFeedbackQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictFeedbackQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict feedback quotas. Empty fields match all.</summary>
public sealed record StrictFeedbackQuotaQuery(string Unit, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(StrictFeedbackQuota item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<StrictFeedbackQuota> Apply(List<StrictFeedbackQuota> items)
    {
        var matched = Collections.Filter(items, (StrictFeedbackQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict feedback quota store.</summary>
public static class StrictFeedbackQuotaStore
{
    public static IStrictFeedbackQuotaStore NewMemoryStrictFeedbackQuotaStore()
    {
        return new MemoryStrictFeedbackQuotaStore(new Dictionary<string, StrictFeedbackQuota>(), new List<string>());
    }

    public static IStrictFeedbackQuotaStore NewCachedStrictFeedbackQuotaStore(MemoryStrictFeedbackQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedStrictFeedbackQuotaStore(inner, StrictFeedbackQuotaCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<StrictFeedbackQuota> FindStrictFeedbackQuotas(IStrictFeedbackQuotaStore store, StrictFeedbackQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
