namespace Acme.Workforce.Compensation.MeritCycle.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary merit cycle allocations by code.</summary>
public interface IPrimaryMeritCycleAllocationStore
{
    PrimaryMeritCycleAllocation? Find(string code);
    void Save(PrimaryMeritCycleAllocation item);
    bool Remove(string code);
    List<PrimaryMeritCycleAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryMeritCycleAllocationStore(Dictionary<string, PrimaryMeritCycleAllocation> items, List<string> order) : IPrimaryMeritCycleAllocationStore
{
    public Dictionary<string, PrimaryMeritCycleAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryMeritCycleAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryMeritCycleAllocation item)
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

    public List<PrimaryMeritCycleAllocation> List()
    {
        var outValue = new List<PrimaryMeritCycleAllocation>();
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
public sealed class CachedPrimaryMeritCycleAllocationStore(MemoryPrimaryMeritCycleAllocationStore inner, PrimaryMeritCycleAllocationCache cache, int hits, int misses) : IPrimaryMeritCycleAllocationStore
{
    public MemoryPrimaryMeritCycleAllocationStore Inner { get; set; } = inner;
    public PrimaryMeritCycleAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryMeritCycleAllocation? Find(string code)
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

    public void Save(PrimaryMeritCycleAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryMeritCycleAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary merit cycle allocations. Empty fields match all.</summary>
public sealed record PrimaryMeritCycleAllocationQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryMeritCycleAllocation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryMeritCycleAllocation> Apply(List<PrimaryMeritCycleAllocation> items)
    {
        var matched = Collections.Filter(items, (PrimaryMeritCycleAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary merit cycle allocation store.</summary>
public static class PrimaryMeritCycleAllocationStore
{
    public static IPrimaryMeritCycleAllocationStore NewMemoryPrimaryMeritCycleAllocationStore()
    {
        return new MemoryPrimaryMeritCycleAllocationStore(
            new Dictionary<string, PrimaryMeritCycleAllocation>(),
            new List<string>());
    }

    public static IPrimaryMeritCycleAllocationStore NewCachedPrimaryMeritCycleAllocationStore(MemoryPrimaryMeritCycleAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryMeritCycleAllocationStore(
            inner,
            PrimaryMeritCycleAllocationCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryMeritCycleAllocation> FindPrimaryMeritCycleAllocations(IPrimaryMeritCycleAllocationStore store, PrimaryMeritCycleAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
