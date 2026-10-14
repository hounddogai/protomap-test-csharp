namespace Acme.Workforce.Orgdesign.Headcount.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount allocations by code.</summary>
public interface IHeadcountAllocationStore
{
    HeadcountAllocation? Find(string code);
    void Save(HeadcountAllocation item);
    bool Remove(string code);
    List<HeadcountAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountAllocationStore(Dictionary<string, HeadcountAllocation> items, List<string> order) : IHeadcountAllocationStore
{
    public Dictionary<string, HeadcountAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountAllocation item)
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

    public List<HeadcountAllocation> List()
    {
        var outValue = new List<HeadcountAllocation>();
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
public sealed class CachedHeadcountAllocationStore(MemoryHeadcountAllocationStore inner, HeadcountAllocationCache cache, int hits, int misses) : IHeadcountAllocationStore
{
    public MemoryHeadcountAllocationStore Inner { get; set; } = inner;
    public HeadcountAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountAllocation? Find(string code)
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

    public void Save(HeadcountAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount allocations. Empty fields match all.</summary>
public sealed record HeadcountAllocationQuery(string Category, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountAllocation item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountAllocation> Apply(List<HeadcountAllocation> items)
    {
        var matched = Collections.Filter(items, (HeadcountAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount allocation store.</summary>
public static class HeadcountAllocationStore
{
    public static IHeadcountAllocationStore NewMemoryHeadcountAllocationStore()
    {
        return new MemoryHeadcountAllocationStore(new Dictionary<string, HeadcountAllocation>(), new List<string>());
    }

    public static IHeadcountAllocationStore NewCachedHeadcountAllocationStore(MemoryHeadcountAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountAllocationStore(inner, HeadcountAllocationCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<HeadcountAllocation> FindHeadcountAllocations(IHeadcountAllocationStore store, HeadcountAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
