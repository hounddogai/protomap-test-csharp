namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling occupancy allocations by code.</summary>
public interface IRollingOccupancyAllocationStore
{
    RollingOccupancyAllocation? Find(string code);
    void Save(RollingOccupancyAllocation item);
    bool Remove(string code);
    List<RollingOccupancyAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingOccupancyAllocationStore(Dictionary<string, RollingOccupancyAllocation> items, List<string> order) : IRollingOccupancyAllocationStore
{
    public Dictionary<string, RollingOccupancyAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingOccupancyAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingOccupancyAllocation item)
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

    public List<RollingOccupancyAllocation> List()
    {
        var outValue = new List<RollingOccupancyAllocation>();
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
public sealed class CachedRollingOccupancyAllocationStore(MemoryRollingOccupancyAllocationStore inner, RollingOccupancyAllocationCache cache, int hits, int misses) : IRollingOccupancyAllocationStore
{
    public MemoryRollingOccupancyAllocationStore Inner { get; set; } = inner;
    public RollingOccupancyAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingOccupancyAllocation? Find(string code)
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

    public void Save(RollingOccupancyAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingOccupancyAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling occupancy allocations. Empty fields match all.</summary>
public sealed record RollingOccupancyAllocationQuery(string Department, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(RollingOccupancyAllocation item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingOccupancyAllocation> Apply(List<RollingOccupancyAllocation> items)
    {
        var matched = Collections.Filter(items, (RollingOccupancyAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling occupancy allocation store.</summary>
public static class RollingOccupancyAllocationStore
{
    public static IRollingOccupancyAllocationStore NewMemoryRollingOccupancyAllocationStore()
    {
        return new MemoryRollingOccupancyAllocationStore(
            new Dictionary<string, RollingOccupancyAllocation>(),
            new List<string>());
    }

    public static IRollingOccupancyAllocationStore NewCachedRollingOccupancyAllocationStore(MemoryRollingOccupancyAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedRollingOccupancyAllocationStore(
            inner,
            RollingOccupancyAllocationCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RollingOccupancyAllocation> FindRollingOccupancyAllocations(IRollingOccupancyAllocationStore store, RollingOccupancyAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
