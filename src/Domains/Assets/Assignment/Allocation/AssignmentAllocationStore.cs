namespace Acme.Workforce.Assets.Assignment.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment allocations by code.</summary>
public interface IAssignmentAllocationStore
{
    AssignmentAllocation? Find(string code);
    void Save(AssignmentAllocation item);
    bool Remove(string code);
    List<AssignmentAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentAllocationStore(Dictionary<string, AssignmentAllocation> items, List<string> order) : IAssignmentAllocationStore
{
    public Dictionary<string, AssignmentAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentAllocation item)
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

    public List<AssignmentAllocation> List()
    {
        var outValue = new List<AssignmentAllocation>();
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
public sealed class CachedAssignmentAllocationStore(MemoryAssignmentAllocationStore inner, AssignmentAllocationCache cache, int hits, int misses) : IAssignmentAllocationStore
{
    public MemoryAssignmentAllocationStore Inner { get; set; } = inner;
    public AssignmentAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentAllocation? Find(string code)
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

    public void Save(AssignmentAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment allocations. Empty fields match all.</summary>
public sealed record AssignmentAllocationQuery(string Label, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentAllocation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentAllocation> Apply(List<AssignmentAllocation> items)
    {
        var matched = Collections.Filter(items, (AssignmentAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment allocation store.</summary>
public static class AssignmentAllocationStore
{
    public static IAssignmentAllocationStore NewMemoryAssignmentAllocationStore()
    {
        return new MemoryAssignmentAllocationStore(
            new Dictionary<string, AssignmentAllocation>(),
            new List<string>());
    }

    public static IAssignmentAllocationStore NewCachedAssignmentAllocationStore(MemoryAssignmentAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentAllocationStore(inner, AssignmentAllocationCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AssignmentAllocation> FindAssignmentAllocations(IAssignmentAllocationStore store, AssignmentAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
