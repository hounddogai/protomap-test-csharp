namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped balance quotas by code.</summary>
public interface ICappedBalanceQuotaStore
{
    CappedBalanceQuota? Find(string code);
    void Save(CappedBalanceQuota item);
    bool Remove(string code);
    List<CappedBalanceQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedBalanceQuotaStore(Dictionary<string, CappedBalanceQuota> items, List<string> order) : ICappedBalanceQuotaStore
{
    public Dictionary<string, CappedBalanceQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedBalanceQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedBalanceQuota item)
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

    public List<CappedBalanceQuota> List()
    {
        var outValue = new List<CappedBalanceQuota>();
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
public sealed class CachedCappedBalanceQuotaStore(MemoryCappedBalanceQuotaStore inner, CappedBalanceQuotaCache cache, int hits, int misses) : ICappedBalanceQuotaStore
{
    public MemoryCappedBalanceQuotaStore Inner { get; set; } = inner;
    public CappedBalanceQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedBalanceQuota? Find(string code)
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

    public void Save(CappedBalanceQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedBalanceQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped balance quotas. Empty fields match all.</summary>
public sealed record CappedBalanceQuotaQuery(string Channel, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(CappedBalanceQuota item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedBalanceQuota> Apply(List<CappedBalanceQuota> items)
    {
        var matched = Collections.Filter(items, (CappedBalanceQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped balance quota store.</summary>
public static class CappedBalanceQuotaStore
{
    public static ICappedBalanceQuotaStore NewMemoryCappedBalanceQuotaStore()
    {
        return new MemoryCappedBalanceQuotaStore(new Dictionary<string, CappedBalanceQuota>(), new List<string>());
    }

    public static ICappedBalanceQuotaStore NewCachedCappedBalanceQuotaStore(MemoryCappedBalanceQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedCappedBalanceQuotaStore(inner, CappedBalanceQuotaCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CappedBalanceQuota> FindCappedBalanceQuotas(ICappedBalanceQuotaStore store, CappedBalanceQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
