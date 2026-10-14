namespace Acme.Workforce.Compensation.EquityGrant.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled equity grant quotas by code.</summary>
public interface IScheduledEquityGrantQuotaStore
{
    ScheduledEquityGrantQuota? Find(string code);
    void Save(ScheduledEquityGrantQuota item);
    bool Remove(string code);
    List<ScheduledEquityGrantQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledEquityGrantQuotaStore(Dictionary<string, ScheduledEquityGrantQuota> items, List<string> order) : IScheduledEquityGrantQuotaStore
{
    public Dictionary<string, ScheduledEquityGrantQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledEquityGrantQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledEquityGrantQuota item)
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

    public List<ScheduledEquityGrantQuota> List()
    {
        var outValue = new List<ScheduledEquityGrantQuota>();
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
public sealed class CachedScheduledEquityGrantQuotaStore(MemoryScheduledEquityGrantQuotaStore inner, ScheduledEquityGrantQuotaCache cache, int hits, int misses) : IScheduledEquityGrantQuotaStore
{
    public MemoryScheduledEquityGrantQuotaStore Inner { get; set; } = inner;
    public ScheduledEquityGrantQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledEquityGrantQuota? Find(string code)
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

    public void Save(ScheduledEquityGrantQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledEquityGrantQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled equity grant quotas. Empty fields match all.</summary>
public sealed record ScheduledEquityGrantQuotaQuery(string Category, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledEquityGrantQuota item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledEquityGrantQuota> Apply(List<ScheduledEquityGrantQuota> items)
    {
        var matched = Collections.Filter(items, (ScheduledEquityGrantQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled equity grant quota store.</summary>
public static class ScheduledEquityGrantQuotaStore
{
    public static IScheduledEquityGrantQuotaStore NewMemoryScheduledEquityGrantQuotaStore()
    {
        return new MemoryScheduledEquityGrantQuotaStore(
            new Dictionary<string, ScheduledEquityGrantQuota>(),
            new List<string>());
    }

    public static IScheduledEquityGrantQuotaStore NewCachedScheduledEquityGrantQuotaStore(MemoryScheduledEquityGrantQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledEquityGrantQuotaStore(
            inner,
            ScheduledEquityGrantQuotaCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledEquityGrantQuota> FindScheduledEquityGrantQuotas(IScheduledEquityGrantQuotaStore store, ScheduledEquityGrantQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
