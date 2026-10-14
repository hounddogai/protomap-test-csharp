namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool policies by code.</summary>
public interface IPoolPolicyStore
{
    PoolPolicy? Find(string code);
    void Save(PoolPolicy item);
    bool Remove(string code);
    List<PoolPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolPolicyStore(Dictionary<string, PoolPolicy> items, List<string> order) : IPoolPolicyStore
{
    public Dictionary<string, PoolPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolPolicy item)
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

    public List<PoolPolicy> List()
    {
        var outValue = new List<PoolPolicy>();
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
public sealed class CachedPoolPolicyStore(MemoryPoolPolicyStore inner, PoolPolicyCache cache, int hits, int misses) : IPoolPolicyStore
{
    public MemoryPoolPolicyStore Inner { get; set; } = inner;
    public PoolPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolPolicy? Find(string code)
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

    public void Save(PoolPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool policies. Empty fields match all.</summary>
public sealed record PoolPolicyQuery(string Department, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(PoolPolicy item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolPolicy> Apply(List<PoolPolicy> items)
    {
        var matched = Collections.Filter(items, (PoolPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool policy store.</summary>
public static class PoolPolicyStore
{
    public static IPoolPolicyStore NewMemoryPoolPolicyStore()
    {
        return new MemoryPoolPolicyStore(new Dictionary<string, PoolPolicy>(), new List<string>());
    }

    public static IPoolPolicyStore NewCachedPoolPolicyStore(MemoryPoolPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedPoolPolicyStore(inner, PoolPolicyCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PoolPolicy> FindPoolPolicies(IPoolPolicyStore store, PoolPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
