namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool thresholds by code.</summary>
public interface IPoolThresholdStore
{
    PoolThreshold? Find(string code);
    void Save(PoolThreshold item);
    bool Remove(string code);
    List<PoolThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolThresholdStore(Dictionary<string, PoolThreshold> items, List<string> order) : IPoolThresholdStore
{
    public Dictionary<string, PoolThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolThreshold item)
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

    public List<PoolThreshold> List()
    {
        var outValue = new List<PoolThreshold>();
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
public sealed class CachedPoolThresholdStore(MemoryPoolThresholdStore inner, PoolThresholdCache cache, int hits, int misses) : IPoolThresholdStore
{
    public MemoryPoolThresholdStore Inner { get; set; } = inner;
    public PoolThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolThreshold? Find(string code)
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

    public void Save(PoolThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool thresholds. Empty fields match all.</summary>
public sealed record PoolThresholdQuery(string Department, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(PoolThreshold item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<PoolThreshold> Apply(List<PoolThreshold> items)
    {
        var matched = Collections.Filter(items, (PoolThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool threshold store.</summary>
public static class PoolThresholdStore
{
    public static IPoolThresholdStore NewMemoryPoolThresholdStore()
    {
        return new MemoryPoolThresholdStore(new Dictionary<string, PoolThreshold>(), new List<string>());
    }

    public static IPoolThresholdStore NewCachedPoolThresholdStore(MemoryPoolThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedPoolThresholdStore(inner, PoolThresholdCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PoolThreshold> FindPoolThresholds(IPoolThresholdStore store, PoolThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
