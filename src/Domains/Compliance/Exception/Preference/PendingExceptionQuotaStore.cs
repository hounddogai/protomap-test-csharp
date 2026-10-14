namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending exception quotas by code.</summary>
public interface IPendingExceptionQuotaStore
{
    PendingExceptionQuota? Find(string code);
    void Save(PendingExceptionQuota item);
    bool Remove(string code);
    List<PendingExceptionQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingExceptionQuotaStore(Dictionary<string, PendingExceptionQuota> items, List<string> order) : IPendingExceptionQuotaStore
{
    public Dictionary<string, PendingExceptionQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingExceptionQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingExceptionQuota item)
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

    public List<PendingExceptionQuota> List()
    {
        var outValue = new List<PendingExceptionQuota>();
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
public sealed class CachedPendingExceptionQuotaStore(MemoryPendingExceptionQuotaStore inner, PendingExceptionQuotaCache cache, int hits, int misses) : IPendingExceptionQuotaStore
{
    public MemoryPendingExceptionQuotaStore Inner { get; set; } = inner;
    public PendingExceptionQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingExceptionQuota? Find(string code)
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

    public void Save(PendingExceptionQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingExceptionQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending exception quotas. Empty fields match all.</summary>
public sealed record PendingExceptionQuotaQuery(string Locale, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(PendingExceptionQuota item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingExceptionQuota> Apply(List<PendingExceptionQuota> items)
    {
        var matched = Collections.Filter(items, (PendingExceptionQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending exception quota store.</summary>
public static class PendingExceptionQuotaStore
{
    public static IPendingExceptionQuotaStore NewMemoryPendingExceptionQuotaStore()
    {
        return new MemoryPendingExceptionQuotaStore(
            new Dictionary<string, PendingExceptionQuota>(),
            new List<string>());
    }

    public static IPendingExceptionQuotaStore NewCachedPendingExceptionQuotaStore(MemoryPendingExceptionQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedPendingExceptionQuotaStore(inner, PendingExceptionQuotaCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PendingExceptionQuota> FindPendingExceptionQuotas(IPendingExceptionQuotaStore store, PendingExceptionQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
