namespace Acme.Workforce.Compensation.BonusPool.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool quotas by code.</summary>
public interface IBonusPoolQuotaStore
{
    BonusPoolQuota? Find(string code);
    void Save(BonusPoolQuota item);
    bool Remove(string code);
    List<BonusPoolQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolQuotaStore(Dictionary<string, BonusPoolQuota> items, List<string> order) : IBonusPoolQuotaStore
{
    public Dictionary<string, BonusPoolQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolQuota item)
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

    public List<BonusPoolQuota> List()
    {
        var outValue = new List<BonusPoolQuota>();
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
public sealed class CachedBonusPoolQuotaStore(MemoryBonusPoolQuotaStore inner, BonusPoolQuotaCache cache, int hits, int misses) : IBonusPoolQuotaStore
{
    public MemoryBonusPoolQuotaStore Inner { get; set; } = inner;
    public BonusPoolQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolQuota? Find(string code)
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

    public void Save(BonusPoolQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool quotas. Empty fields match all.</summary>
public sealed record BonusPoolQuotaQuery(string Locale, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolQuota item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolQuota> Apply(List<BonusPoolQuota> items)
    {
        var matched = Collections.Filter(items, (BonusPoolQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool quota store.</summary>
public static class BonusPoolQuotaStore
{
    public static IBonusPoolQuotaStore NewMemoryBonusPoolQuotaStore()
    {
        return new MemoryBonusPoolQuotaStore(new Dictionary<string, BonusPoolQuota>(), new List<string>());
    }

    public static IBonusPoolQuotaStore NewCachedBonusPoolQuotaStore(MemoryBonusPoolQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolQuotaStore(inner, BonusPoolQuotaCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BonusPoolQuota> FindBonusPoolQuotas(IBonusPoolQuotaStore store, BonusPoolQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
