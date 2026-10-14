namespace Acme.Workforce.Absence.Policy.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy allocations by code.</summary>
public interface IPolicyAllocationStore
{
    PolicyAllocation? Find(string code);
    void Save(PolicyAllocation item);
    bool Remove(string code);
    List<PolicyAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAllocationStore(Dictionary<string, PolicyAllocation> items, List<string> order) : IPolicyAllocationStore
{
    public Dictionary<string, PolicyAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAllocation item)
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

    public List<PolicyAllocation> List()
    {
        var outValue = new List<PolicyAllocation>();
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
public sealed class CachedPolicyAllocationStore(MemoryPolicyAllocationStore inner, PolicyAllocationCache cache, int hits, int misses) : IPolicyAllocationStore
{
    public MemoryPolicyAllocationStore Inner { get; set; } = inner;
    public PolicyAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAllocation? Find(string code)
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

    public void Save(PolicyAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy allocations. Empty fields match all.</summary>
public sealed record PolicyAllocationQuery(string Label, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAllocation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAllocation> Apply(List<PolicyAllocation> items)
    {
        var matched = Collections.Filter(items, (PolicyAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy allocation store.</summary>
public static class PolicyAllocationStore
{
    public static IPolicyAllocationStore NewMemoryPolicyAllocationStore()
    {
        return new MemoryPolicyAllocationStore(new Dictionary<string, PolicyAllocation>(), new List<string>());
    }

    public static IPolicyAllocationStore NewCachedPolicyAllocationStore(MemoryPolicyAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAllocationStore(inner, PolicyAllocationCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PolicyAllocation> FindPolicyAllocations(IPolicyAllocationStore store, PolicyAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
