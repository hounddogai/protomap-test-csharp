namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback offboarding allocations by code.</summary>
public interface IFallbackOffboardingAllocationStore
{
    FallbackOffboardingAllocation? Find(string code);
    void Save(FallbackOffboardingAllocation item);
    bool Remove(string code);
    List<FallbackOffboardingAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackOffboardingAllocationStore(Dictionary<string, FallbackOffboardingAllocation> items, List<string> order) : IFallbackOffboardingAllocationStore
{
    public Dictionary<string, FallbackOffboardingAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackOffboardingAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackOffboardingAllocation item)
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

    public List<FallbackOffboardingAllocation> List()
    {
        var outValue = new List<FallbackOffboardingAllocation>();
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
public sealed class CachedFallbackOffboardingAllocationStore(MemoryFallbackOffboardingAllocationStore inner, FallbackOffboardingAllocationCache cache, int hits, int misses) : IFallbackOffboardingAllocationStore
{
    public MemoryFallbackOffboardingAllocationStore Inner { get; set; } = inner;
    public FallbackOffboardingAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackOffboardingAllocation? Find(string code)
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

    public void Save(FallbackOffboardingAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackOffboardingAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback offboarding allocations. Empty fields match all.</summary>
public sealed record FallbackOffboardingAllocationQuery(string Source, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackOffboardingAllocation item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackOffboardingAllocation> Apply(List<FallbackOffboardingAllocation> items)
    {
        var matched = Collections.Filter(items, (FallbackOffboardingAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback offboarding allocation store.</summary>
public static class FallbackOffboardingAllocationStore
{
    public static IFallbackOffboardingAllocationStore NewMemoryFallbackOffboardingAllocationStore()
    {
        return new MemoryFallbackOffboardingAllocationStore(
            new Dictionary<string, FallbackOffboardingAllocation>(),
            new List<string>());
    }

    public static IFallbackOffboardingAllocationStore NewCachedFallbackOffboardingAllocationStore(MemoryFallbackOffboardingAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackOffboardingAllocationStore(
            inner,
            FallbackOffboardingAllocationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackOffboardingAllocation> FindFallbackOffboardingAllocations(IFallbackOffboardingAllocationStore store, FallbackOffboardingAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
