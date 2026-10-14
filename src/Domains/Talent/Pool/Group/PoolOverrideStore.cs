namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool overrides by code.</summary>
public interface IPoolOverrideStore
{
    PoolOverride? Find(string code);
    void Save(PoolOverride item);
    bool Remove(string code);
    List<PoolOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolOverrideStore(Dictionary<string, PoolOverride> items, List<string> order) : IPoolOverrideStore
{
    public Dictionary<string, PoolOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolOverride item)
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

    public List<PoolOverride> List()
    {
        var outValue = new List<PoolOverride>();
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
public sealed class CachedPoolOverrideStore(MemoryPoolOverrideStore inner, PoolOverrideCache cache, int hits, int misses) : IPoolOverrideStore
{
    public MemoryPoolOverrideStore Inner { get; set; } = inner;
    public PoolOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolOverride? Find(string code)
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

    public void Save(PoolOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool overrides. Empty fields match all.</summary>
public sealed record PoolOverrideQuery(string Name, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(PoolOverride item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolOverride> Apply(List<PoolOverride> items)
    {
        var matched = Collections.Filter(items, (PoolOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool override store.</summary>
public static class PoolOverrideStore
{
    public static IPoolOverrideStore NewMemoryPoolOverrideStore()
    {
        return new MemoryPoolOverrideStore(new Dictionary<string, PoolOverride>(), new List<string>());
    }

    public static IPoolOverrideStore NewCachedPoolOverrideStore(MemoryPoolOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedPoolOverrideStore(inner, PoolOverrideCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PoolOverride> FindPoolOverrides(IPoolOverrideStore store, PoolOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
