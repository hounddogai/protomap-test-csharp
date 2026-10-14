namespace Acme.Workforce.Absence.Balance.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling balance quotas by code.</summary>
public interface IRollingBalanceQuotaStore
{
    RollingBalanceQuota? Find(string code);
    void Save(RollingBalanceQuota item);
    bool Remove(string code);
    List<RollingBalanceQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingBalanceQuotaStore(Dictionary<string, RollingBalanceQuota> items, List<string> order) : IRollingBalanceQuotaStore
{
    public Dictionary<string, RollingBalanceQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingBalanceQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingBalanceQuota item)
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

    public List<RollingBalanceQuota> List()
    {
        var outValue = new List<RollingBalanceQuota>();
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
public sealed class CachedRollingBalanceQuotaStore(MemoryRollingBalanceQuotaStore inner, RollingBalanceQuotaCache cache, int hits, int misses) : IRollingBalanceQuotaStore
{
    public MemoryRollingBalanceQuotaStore Inner { get; set; } = inner;
    public RollingBalanceQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingBalanceQuota? Find(string code)
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

    public void Save(RollingBalanceQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingBalanceQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling balance quotas. Empty fields match all.</summary>
public sealed record RollingBalanceQuotaQuery(string Unit, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(RollingBalanceQuota item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingBalanceQuota> Apply(List<RollingBalanceQuota> items)
    {
        var matched = Collections.Filter(items, (RollingBalanceQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling balance quota store.</summary>
public static class RollingBalanceQuotaStore
{
    public static IRollingBalanceQuotaStore NewMemoryRollingBalanceQuotaStore()
    {
        return new MemoryRollingBalanceQuotaStore(new Dictionary<string, RollingBalanceQuota>(), new List<string>());
    }

    public static IRollingBalanceQuotaStore NewCachedRollingBalanceQuotaStore(MemoryRollingBalanceQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedRollingBalanceQuotaStore(inner, RollingBalanceQuotaCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RollingBalanceQuota> FindRollingBalanceQuotas(IRollingBalanceQuotaStore store, RollingBalanceQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
