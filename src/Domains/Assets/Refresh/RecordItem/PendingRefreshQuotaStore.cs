namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending refresh quotas by code.</summary>
public interface IPendingRefreshQuotaStore
{
    PendingRefreshQuota? Find(string code);
    void Save(PendingRefreshQuota item);
    bool Remove(string code);
    List<PendingRefreshQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingRefreshQuotaStore(Dictionary<string, PendingRefreshQuota> items, List<string> order) : IPendingRefreshQuotaStore
{
    public Dictionary<string, PendingRefreshQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingRefreshQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingRefreshQuota item)
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

    public List<PendingRefreshQuota> List()
    {
        var outValue = new List<PendingRefreshQuota>();
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
public sealed class CachedPendingRefreshQuotaStore(MemoryPendingRefreshQuotaStore inner, PendingRefreshQuotaCache cache, int hits, int misses) : IPendingRefreshQuotaStore
{
    public MemoryPendingRefreshQuotaStore Inner { get; set; } = inner;
    public PendingRefreshQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingRefreshQuota? Find(string code)
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

    public void Save(PendingRefreshQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingRefreshQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending refresh quotas. Empty fields match all.</summary>
public sealed record PendingRefreshQuotaQuery(string Title, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(PendingRefreshQuota item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingRefreshQuota> Apply(List<PendingRefreshQuota> items)
    {
        var matched = Collections.Filter(items, (PendingRefreshQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending refresh quota store.</summary>
public static class PendingRefreshQuotaStore
{
    public static IPendingRefreshQuotaStore NewMemoryPendingRefreshQuotaStore()
    {
        return new MemoryPendingRefreshQuotaStore(new Dictionary<string, PendingRefreshQuota>(), new List<string>());
    }

    public static IPendingRefreshQuotaStore NewCachedPendingRefreshQuotaStore(MemoryPendingRefreshQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedPendingRefreshQuotaStore(inner, PendingRefreshQuotaCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PendingRefreshQuota> FindPendingRefreshQuotas(IPendingRefreshQuotaStore store, PendingRefreshQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
