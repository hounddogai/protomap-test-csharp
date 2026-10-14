namespace Acme.Workforce.Orgdesign.Headcount.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy headcount quotas by code.</summary>
public interface ILegacyHeadcountQuotaStore
{
    LegacyHeadcountQuota? Find(string code);
    void Save(LegacyHeadcountQuota item);
    bool Remove(string code);
    List<LegacyHeadcountQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyHeadcountQuotaStore(Dictionary<string, LegacyHeadcountQuota> items, List<string> order) : ILegacyHeadcountQuotaStore
{
    public Dictionary<string, LegacyHeadcountQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyHeadcountQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyHeadcountQuota item)
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

    public List<LegacyHeadcountQuota> List()
    {
        var outValue = new List<LegacyHeadcountQuota>();
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
public sealed class CachedLegacyHeadcountQuotaStore(MemoryLegacyHeadcountQuotaStore inner, LegacyHeadcountQuotaCache cache, int hits, int misses) : ILegacyHeadcountQuotaStore
{
    public MemoryLegacyHeadcountQuotaStore Inner { get; set; } = inner;
    public LegacyHeadcountQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyHeadcountQuota? Find(string code)
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

    public void Save(LegacyHeadcountQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyHeadcountQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy headcount quotas. Empty fields match all.</summary>
public sealed record LegacyHeadcountQuotaQuery(string Department, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyHeadcountQuota item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyHeadcountQuota> Apply(List<LegacyHeadcountQuota> items)
    {
        var matched = Collections.Filter(items, (LegacyHeadcountQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy headcount quota store.</summary>
public static class LegacyHeadcountQuotaStore
{
    public static ILegacyHeadcountQuotaStore NewMemoryLegacyHeadcountQuotaStore()
    {
        return new MemoryLegacyHeadcountQuotaStore(
            new Dictionary<string, LegacyHeadcountQuota>(),
            new List<string>());
    }

    public static ILegacyHeadcountQuotaStore NewCachedLegacyHeadcountQuotaStore(MemoryLegacyHeadcountQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyHeadcountQuotaStore(inner, LegacyHeadcountQuotaCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LegacyHeadcountQuota> FindLegacyHeadcountQuotas(ILegacyHeadcountQuotaStore store, LegacyHeadcountQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
