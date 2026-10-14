namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient desk allocations by code.</summary>
public interface ILenientDeskAllocationStore
{
    LenientDeskAllocation? Find(string code);
    void Save(LenientDeskAllocation item);
    bool Remove(string code);
    List<LenientDeskAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientDeskAllocationStore(Dictionary<string, LenientDeskAllocation> items, List<string> order) : ILenientDeskAllocationStore
{
    public Dictionary<string, LenientDeskAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientDeskAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientDeskAllocation item)
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

    public List<LenientDeskAllocation> List()
    {
        var outValue = new List<LenientDeskAllocation>();
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
public sealed class CachedLenientDeskAllocationStore(MemoryLenientDeskAllocationStore inner, LenientDeskAllocationCache cache, int hits, int misses) : ILenientDeskAllocationStore
{
    public MemoryLenientDeskAllocationStore Inner { get; set; } = inner;
    public LenientDeskAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientDeskAllocation? Find(string code)
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

    public void Save(LenientDeskAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientDeskAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient desk allocations. Empty fields match all.</summary>
public sealed record LenientDeskAllocationQuery(string Title, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(LenientDeskAllocation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientDeskAllocation> Apply(List<LenientDeskAllocation> items)
    {
        var matched = Collections.Filter(items, (LenientDeskAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient desk allocation store.</summary>
public static class LenientDeskAllocationStore
{
    public static ILenientDeskAllocationStore NewMemoryLenientDeskAllocationStore()
    {
        return new MemoryLenientDeskAllocationStore(
            new Dictionary<string, LenientDeskAllocation>(),
            new List<string>());
    }

    public static ILenientDeskAllocationStore NewCachedLenientDeskAllocationStore(MemoryLenientDeskAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedLenientDeskAllocationStore(inner, LenientDeskAllocationCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LenientDeskAllocation> FindLenientDeskAllocations(ILenientDeskAllocationStore store, LenientDeskAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
