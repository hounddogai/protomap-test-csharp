namespace Acme.Workforce.Contractors.Rate.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback rate allocations by code.</summary>
public interface IFallbackRateAllocationStore
{
    FallbackRateAllocation? Find(string code);
    void Save(FallbackRateAllocation item);
    bool Remove(string code);
    List<FallbackRateAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackRateAllocationStore(Dictionary<string, FallbackRateAllocation> items, List<string> order) : IFallbackRateAllocationStore
{
    public Dictionary<string, FallbackRateAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackRateAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackRateAllocation item)
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

    public List<FallbackRateAllocation> List()
    {
        var outValue = new List<FallbackRateAllocation>();
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
public sealed class CachedFallbackRateAllocationStore(MemoryFallbackRateAllocationStore inner, FallbackRateAllocationCache cache, int hits, int misses) : IFallbackRateAllocationStore
{
    public MemoryFallbackRateAllocationStore Inner { get; set; } = inner;
    public FallbackRateAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackRateAllocation? Find(string code)
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

    public void Save(FallbackRateAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackRateAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback rate allocations. Empty fields match all.</summary>
public sealed record FallbackRateAllocationQuery(string Channel, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackRateAllocation item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackRateAllocation> Apply(List<FallbackRateAllocation> items)
    {
        var matched = Collections.Filter(items, (FallbackRateAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback rate allocation store.</summary>
public static class FallbackRateAllocationStore
{
    public static IFallbackRateAllocationStore NewMemoryFallbackRateAllocationStore()
    {
        return new MemoryFallbackRateAllocationStore(
            new Dictionary<string, FallbackRateAllocation>(),
            new List<string>());
    }

    public static IFallbackRateAllocationStore NewCachedFallbackRateAllocationStore(MemoryFallbackRateAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackRateAllocationStore(
            inner,
            FallbackRateAllocationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<FallbackRateAllocation> FindFallbackRateAllocations(IFallbackRateAllocationStore store, FallbackRateAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
