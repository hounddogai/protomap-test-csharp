namespace Acme.Workforce.Compensation.PayBand.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending pay band quotas by code.</summary>
public interface IPendingPayBandQuotaStore
{
    PendingPayBandQuota? Find(string code);
    void Save(PendingPayBandQuota item);
    bool Remove(string code);
    List<PendingPayBandQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingPayBandQuotaStore(Dictionary<string, PendingPayBandQuota> items, List<string> order) : IPendingPayBandQuotaStore
{
    public Dictionary<string, PendingPayBandQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingPayBandQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingPayBandQuota item)
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

    public List<PendingPayBandQuota> List()
    {
        var outValue = new List<PendingPayBandQuota>();
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
public sealed class CachedPendingPayBandQuotaStore(MemoryPendingPayBandQuotaStore inner, PendingPayBandQuotaCache cache, int hits, int misses) : IPendingPayBandQuotaStore
{
    public MemoryPendingPayBandQuotaStore Inner { get; set; } = inner;
    public PendingPayBandQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingPayBandQuota? Find(string code)
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

    public void Save(PendingPayBandQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingPayBandQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending pay band quotas. Empty fields match all.</summary>
public sealed record PendingPayBandQuotaQuery(string Source, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(PendingPayBandQuota item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingPayBandQuota> Apply(List<PendingPayBandQuota> items)
    {
        var matched = Collections.Filter(items, (PendingPayBandQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending pay band quota store.</summary>
public static class PendingPayBandQuotaStore
{
    public static IPendingPayBandQuotaStore NewMemoryPendingPayBandQuotaStore()
    {
        return new MemoryPendingPayBandQuotaStore(new Dictionary<string, PendingPayBandQuota>(), new List<string>());
    }

    public static IPendingPayBandQuotaStore NewCachedPendingPayBandQuotaStore(MemoryPendingPayBandQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedPendingPayBandQuotaStore(inner, PendingPayBandQuotaCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PendingPayBandQuota> FindPendingPayBandQuotas(IPendingPayBandQuotaStore store, PendingPayBandQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
