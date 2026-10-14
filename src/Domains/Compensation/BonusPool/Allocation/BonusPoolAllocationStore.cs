namespace Acme.Workforce.Compensation.BonusPool.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool allocations by code.</summary>
public interface IBonusPoolAllocationStore
{
    BonusPoolAllocation? Find(string code);
    void Save(BonusPoolAllocation item);
    bool Remove(string code);
    List<BonusPoolAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolAllocationStore(Dictionary<string, BonusPoolAllocation> items, List<string> order) : IBonusPoolAllocationStore
{
    public Dictionary<string, BonusPoolAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolAllocation item)
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

    public List<BonusPoolAllocation> List()
    {
        var outValue = new List<BonusPoolAllocation>();
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
public sealed class CachedBonusPoolAllocationStore(MemoryBonusPoolAllocationStore inner, BonusPoolAllocationCache cache, int hits, int misses) : IBonusPoolAllocationStore
{
    public MemoryBonusPoolAllocationStore Inner { get; set; } = inner;
    public BonusPoolAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolAllocation? Find(string code)
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

    public void Save(BonusPoolAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool allocations. Empty fields match all.</summary>
public sealed record BonusPoolAllocationQuery(string Name, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolAllocation item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolAllocation> Apply(List<BonusPoolAllocation> items)
    {
        var matched = Collections.Filter(items, (BonusPoolAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool allocation store.</summary>
public static class BonusPoolAllocationStore
{
    public static IBonusPoolAllocationStore NewMemoryBonusPoolAllocationStore()
    {
        return new MemoryBonusPoolAllocationStore(new Dictionary<string, BonusPoolAllocation>(), new List<string>());
    }

    public static IBonusPoolAllocationStore NewCachedBonusPoolAllocationStore(MemoryBonusPoolAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolAllocationStore(inner, BonusPoolAllocationCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BonusPoolAllocation> FindBonusPoolAllocations(IBonusPoolAllocationStore store, BonusPoolAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
