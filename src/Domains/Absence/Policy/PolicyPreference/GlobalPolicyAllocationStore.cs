namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global policy allocations by code.</summary>
public interface IGlobalPolicyAllocationStore
{
    GlobalPolicyAllocation? Find(string code);
    void Save(GlobalPolicyAllocation item);
    bool Remove(string code);
    List<GlobalPolicyAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPolicyAllocationStore(Dictionary<string, GlobalPolicyAllocation> items, List<string> order) : IGlobalPolicyAllocationStore
{
    public Dictionary<string, GlobalPolicyAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPolicyAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPolicyAllocation item)
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

    public List<GlobalPolicyAllocation> List()
    {
        var outValue = new List<GlobalPolicyAllocation>();
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
public sealed class CachedGlobalPolicyAllocationStore(MemoryGlobalPolicyAllocationStore inner, GlobalPolicyAllocationCache cache, int hits, int misses) : IGlobalPolicyAllocationStore
{
    public MemoryGlobalPolicyAllocationStore Inner { get; set; } = inner;
    public GlobalPolicyAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPolicyAllocation? Find(string code)
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

    public void Save(GlobalPolicyAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPolicyAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global policy allocations. Empty fields match all.</summary>
public sealed record GlobalPolicyAllocationQuery(string Name, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPolicyAllocation item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<GlobalPolicyAllocation> Apply(List<GlobalPolicyAllocation> items)
    {
        var matched = Collections.Filter(items, (GlobalPolicyAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global policy allocation store.</summary>
public static class GlobalPolicyAllocationStore
{
    public static IGlobalPolicyAllocationStore NewMemoryGlobalPolicyAllocationStore()
    {
        return new MemoryGlobalPolicyAllocationStore(
            new Dictionary<string, GlobalPolicyAllocation>(),
            new List<string>());
    }

    public static IGlobalPolicyAllocationStore NewCachedGlobalPolicyAllocationStore(MemoryGlobalPolicyAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPolicyAllocationStore(
            inner,
            GlobalPolicyAllocationCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<GlobalPolicyAllocation> FindGlobalPolicyAllocations(IGlobalPolicyAllocationStore store, GlobalPolicyAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
