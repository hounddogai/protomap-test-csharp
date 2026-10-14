namespace Acme.Workforce.Talent.Pool.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool quotas by code.</summary>
public interface IPoolQuotaStore
{
    PoolQuota? Find(string code);
    void Save(PoolQuota item);
    bool Remove(string code);
    List<PoolQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolQuotaStore(Dictionary<string, PoolQuota> items, List<string> order) : IPoolQuotaStore
{
    public Dictionary<string, PoolQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolQuota item)
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

    public List<PoolQuota> List()
    {
        var outValue = new List<PoolQuota>();
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
public sealed class CachedPoolQuotaStore(MemoryPoolQuotaStore inner, PoolQuotaCache cache, int hits, int misses) : IPoolQuotaStore
{
    public MemoryPoolQuotaStore Inner { get; set; } = inner;
    public PoolQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolQuota? Find(string code)
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

    public void Save(PoolQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool quotas. Empty fields match all.</summary>
public sealed record PoolQuotaQuery(string Label, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(PoolQuota item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolQuota> Apply(List<PoolQuota> items)
    {
        var matched = Collections.Filter(items, (PoolQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool quota store.</summary>
public static class PoolQuotaStore
{
    public static IPoolQuotaStore NewMemoryPoolQuotaStore()
    {
        return new MemoryPoolQuotaStore(new Dictionary<string, PoolQuota>(), new List<string>());
    }

    public static IPoolQuotaStore NewCachedPoolQuotaStore(MemoryPoolQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedPoolQuotaStore(inner, PoolQuotaCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PoolQuota> FindPoolQuotas(IPoolQuotaStore store, PoolQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
