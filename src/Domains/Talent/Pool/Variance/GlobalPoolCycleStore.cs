namespace Acme.Workforce.Talent.Pool.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global pool cycles by code.</summary>
public interface IGlobalPoolCycleStore
{
    GlobalPoolCycle? Find(string code);
    void Save(GlobalPoolCycle item);
    bool Remove(string code);
    List<GlobalPoolCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPoolCycleStore(Dictionary<string, GlobalPoolCycle> items, List<string> order) : IGlobalPoolCycleStore
{
    public Dictionary<string, GlobalPoolCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPoolCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPoolCycle item)
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

    public List<GlobalPoolCycle> List()
    {
        var outValue = new List<GlobalPoolCycle>();
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
public sealed class CachedGlobalPoolCycleStore(MemoryGlobalPoolCycleStore inner, GlobalPoolCycleCache cache, int hits, int misses) : IGlobalPoolCycleStore
{
    public MemoryGlobalPoolCycleStore Inner { get; set; } = inner;
    public GlobalPoolCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPoolCycle? Find(string code)
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

    public void Save(GlobalPoolCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPoolCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global pool cycles. Empty fields match all.</summary>
public sealed record GlobalPoolCycleQuery(string Unit, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPoolCycle item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalPoolCycle> Apply(List<GlobalPoolCycle> items)
    {
        var matched = Collections.Filter(items, (GlobalPoolCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global pool cycle store.</summary>
public static class GlobalPoolCycleStore
{
    public static IGlobalPoolCycleStore NewMemoryGlobalPoolCycleStore()
    {
        return new MemoryGlobalPoolCycleStore(new Dictionary<string, GlobalPoolCycle>(), new List<string>());
    }

    public static IGlobalPoolCycleStore NewCachedGlobalPoolCycleStore(MemoryGlobalPoolCycleStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPoolCycleStore(inner, GlobalPoolCycleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<GlobalPoolCycle> FindGlobalPoolCycles(IGlobalPoolCycleStore store, GlobalPoolCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
