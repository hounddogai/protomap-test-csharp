namespace Acme.Workforce.Contractors.Offboarding.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding allocations by code.</summary>
public interface IOffboardingAllocationStore
{
    OffboardingAllocation? Find(string code);
    void Save(OffboardingAllocation item);
    bool Remove(string code);
    List<OffboardingAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingAllocationStore(Dictionary<string, OffboardingAllocation> items, List<string> order) : IOffboardingAllocationStore
{
    public Dictionary<string, OffboardingAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingAllocation item)
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

    public List<OffboardingAllocation> List()
    {
        var outValue = new List<OffboardingAllocation>();
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
public sealed class CachedOffboardingAllocationStore(MemoryOffboardingAllocationStore inner, OffboardingAllocationCache cache, int hits, int misses) : IOffboardingAllocationStore
{
    public MemoryOffboardingAllocationStore Inner { get; set; } = inner;
    public OffboardingAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingAllocation? Find(string code)
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

    public void Save(OffboardingAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding allocations. Empty fields match all.</summary>
public sealed record OffboardingAllocationQuery(string Source, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingAllocation item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OffboardingAllocation> Apply(List<OffboardingAllocation> items)
    {
        var matched = Collections.Filter(items, (OffboardingAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding allocation store.</summary>
public static class OffboardingAllocationStore
{
    public static IOffboardingAllocationStore NewMemoryOffboardingAllocationStore()
    {
        return new MemoryOffboardingAllocationStore(
            new Dictionary<string, OffboardingAllocation>(),
            new List<string>());
    }

    public static IOffboardingAllocationStore NewCachedOffboardingAllocationStore(MemoryOffboardingAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingAllocationStore(inner, OffboardingAllocationCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<OffboardingAllocation> FindOffboardingAllocations(IOffboardingAllocationStore store, OffboardingAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
