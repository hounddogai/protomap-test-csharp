namespace Acme.Workforce.Compensation.BonusPool.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered bonus pool quotas by code.</summary>
public interface ITieredBonusPoolQuotaStore
{
    TieredBonusPoolQuota? Find(string code);
    void Save(TieredBonusPoolQuota item);
    bool Remove(string code);
    List<TieredBonusPoolQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredBonusPoolQuotaStore(Dictionary<string, TieredBonusPoolQuota> items, List<string> order) : ITieredBonusPoolQuotaStore
{
    public Dictionary<string, TieredBonusPoolQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredBonusPoolQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredBonusPoolQuota item)
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

    public List<TieredBonusPoolQuota> List()
    {
        var outValue = new List<TieredBonusPoolQuota>();
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
public sealed class CachedTieredBonusPoolQuotaStore(MemoryTieredBonusPoolQuotaStore inner, TieredBonusPoolQuotaCache cache, int hits, int misses) : ITieredBonusPoolQuotaStore
{
    public MemoryTieredBonusPoolQuotaStore Inner { get; set; } = inner;
    public TieredBonusPoolQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredBonusPoolQuota? Find(string code)
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

    public void Save(TieredBonusPoolQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredBonusPoolQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered bonus pool quotas. Empty fields match all.</summary>
public sealed record TieredBonusPoolQuotaQuery(string Category, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(TieredBonusPoolQuota item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredBonusPoolQuota> Apply(List<TieredBonusPoolQuota> items)
    {
        var matched = Collections.Filter(items, (TieredBonusPoolQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered bonus pool quota store.</summary>
public static class TieredBonusPoolQuotaStore
{
    public static ITieredBonusPoolQuotaStore NewMemoryTieredBonusPoolQuotaStore()
    {
        return new MemoryTieredBonusPoolQuotaStore(
            new Dictionary<string, TieredBonusPoolQuota>(),
            new List<string>());
    }

    public static ITieredBonusPoolQuotaStore NewCachedTieredBonusPoolQuotaStore(MemoryTieredBonusPoolQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedTieredBonusPoolQuotaStore(inner, TieredBonusPoolQuotaCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TieredBonusPoolQuota> FindTieredBonusPoolQuotas(ITieredBonusPoolQuotaStore store, TieredBonusPoolQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
