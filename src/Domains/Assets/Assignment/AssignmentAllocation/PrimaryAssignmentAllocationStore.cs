namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary assignment allocations by code.</summary>
public interface IPrimaryAssignmentAllocationStore
{
    PrimaryAssignmentAllocation? Find(string code);
    void Save(PrimaryAssignmentAllocation item);
    bool Remove(string code);
    List<PrimaryAssignmentAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAssignmentAllocationStore(Dictionary<string, PrimaryAssignmentAllocation> items, List<string> order) : IPrimaryAssignmentAllocationStore
{
    public Dictionary<string, PrimaryAssignmentAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAssignmentAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAssignmentAllocation item)
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

    public List<PrimaryAssignmentAllocation> List()
    {
        var outValue = new List<PrimaryAssignmentAllocation>();
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
public sealed class CachedPrimaryAssignmentAllocationStore(MemoryPrimaryAssignmentAllocationStore inner, PrimaryAssignmentAllocationCache cache, int hits, int misses) : IPrimaryAssignmentAllocationStore
{
    public MemoryPrimaryAssignmentAllocationStore Inner { get; set; } = inner;
    public PrimaryAssignmentAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAssignmentAllocation? Find(string code)
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

    public void Save(PrimaryAssignmentAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAssignmentAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary assignment allocations. Empty fields match all.</summary>
public sealed record PrimaryAssignmentAllocationQuery(string Name, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAssignmentAllocation item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryAssignmentAllocation> Apply(List<PrimaryAssignmentAllocation> items)
    {
        var matched = Collections.Filter(items, (PrimaryAssignmentAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary assignment allocation store.</summary>
public static class PrimaryAssignmentAllocationStore
{
    public static IPrimaryAssignmentAllocationStore NewMemoryPrimaryAssignmentAllocationStore()
    {
        return new MemoryPrimaryAssignmentAllocationStore(
            new Dictionary<string, PrimaryAssignmentAllocation>(),
            new List<string>());
    }

    public static IPrimaryAssignmentAllocationStore NewCachedPrimaryAssignmentAllocationStore(MemoryPrimaryAssignmentAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAssignmentAllocationStore(
            inner,
            PrimaryAssignmentAllocationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryAssignmentAllocation> FindPrimaryAssignmentAllocations(IPrimaryAssignmentAllocationStore store, PrimaryAssignmentAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
