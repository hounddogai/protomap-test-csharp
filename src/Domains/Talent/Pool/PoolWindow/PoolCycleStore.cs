namespace Acme.Workforce.Talent.Pool.PoolWindow;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool cycles by code.</summary>
public interface IPoolCycleStore
{
    PoolCycle? Find(string code);
    void Save(PoolCycle item);
    bool Remove(string code);
    List<PoolCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolCycleStore(Dictionary<string, PoolCycle> items, List<string> order) : IPoolCycleStore
{
    public Dictionary<string, PoolCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolCycle item)
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

    public List<PoolCycle> List()
    {
        var outValue = new List<PoolCycle>();
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
public sealed class CachedPoolCycleStore(MemoryPoolCycleStore inner, PoolCycleCache cache, int hits, int misses) : IPoolCycleStore
{
    public MemoryPoolCycleStore Inner { get; set; } = inner;
    public PoolCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolCycle? Find(string code)
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

    public void Save(PoolCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool cycles. Empty fields match all.</summary>
public sealed record PoolCycleQuery(string Team, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(PoolCycle item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolCycle> Apply(List<PoolCycle> items)
    {
        var matched = Collections.Filter(items, (PoolCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool cycle store.</summary>
public static class PoolCycleStore
{
    public static IPoolCycleStore NewMemoryPoolCycleStore()
    {
        return new MemoryPoolCycleStore(new Dictionary<string, PoolCycle>(), new List<string>());
    }

    public static IPoolCycleStore NewCachedPoolCycleStore(MemoryPoolCycleStore inner, int ttl, IClock clock)
    {
        return new CachedPoolCycleStore(inner, PoolCycleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PoolCycle> FindPoolCycles(IPoolCycleStore store, PoolCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
