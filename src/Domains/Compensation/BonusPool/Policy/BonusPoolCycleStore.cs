namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool cycles by code.</summary>
public interface IBonusPoolCycleStore
{
    BonusPoolCycle? Find(string code);
    void Save(BonusPoolCycle item);
    bool Remove(string code);
    List<BonusPoolCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolCycleStore(Dictionary<string, BonusPoolCycle> items, List<string> order) : IBonusPoolCycleStore
{
    public Dictionary<string, BonusPoolCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolCycle item)
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

    public List<BonusPoolCycle> List()
    {
        var outValue = new List<BonusPoolCycle>();
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
public sealed class CachedBonusPoolCycleStore(MemoryBonusPoolCycleStore inner, BonusPoolCycleCache cache, int hits, int misses) : IBonusPoolCycleStore
{
    public MemoryBonusPoolCycleStore Inner { get; set; } = inner;
    public BonusPoolCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolCycle? Find(string code)
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

    public void Save(BonusPoolCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool cycles. Empty fields match all.</summary>
public sealed record BonusPoolCycleQuery(string Unit, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolCycle item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolCycle> Apply(List<BonusPoolCycle> items)
    {
        var matched = Collections.Filter(items, (BonusPoolCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool cycle store.</summary>
public static class BonusPoolCycleStore
{
    public static IBonusPoolCycleStore NewMemoryBonusPoolCycleStore()
    {
        return new MemoryBonusPoolCycleStore(new Dictionary<string, BonusPoolCycle>(), new List<string>());
    }

    public static IBonusPoolCycleStore NewCachedBonusPoolCycleStore(MemoryBonusPoolCycleStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolCycleStore(inner, BonusPoolCycleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BonusPoolCycle> FindBonusPoolCycles(IBonusPoolCycleStore store, BonusPoolCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
