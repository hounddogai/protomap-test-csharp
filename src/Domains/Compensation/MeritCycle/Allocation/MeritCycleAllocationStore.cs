namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle allocations by code.</summary>
public interface IMeritCycleAllocationStore
{
    MeritCycleAllocation? Find(string code);
    void Save(MeritCycleAllocation item);
    bool Remove(string code);
    List<MeritCycleAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleAllocationStore(Dictionary<string, MeritCycleAllocation> items, List<string> order) : IMeritCycleAllocationStore
{
    public Dictionary<string, MeritCycleAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleAllocation item)
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

    public List<MeritCycleAllocation> List()
    {
        var outValue = new List<MeritCycleAllocation>();
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
public sealed class CachedMeritCycleAllocationStore(MemoryMeritCycleAllocationStore inner, MeritCycleAllocationCache cache, int hits, int misses) : IMeritCycleAllocationStore
{
    public MemoryMeritCycleAllocationStore Inner { get; set; } = inner;
    public MeritCycleAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleAllocation? Find(string code)
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

    public void Save(MeritCycleAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle allocations. Empty fields match all.</summary>
public sealed record MeritCycleAllocationQuery(string Team, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleAllocation item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleAllocation> Apply(List<MeritCycleAllocation> items)
    {
        var matched = Collections.Filter(items, (MeritCycleAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle allocation store.</summary>
public static class MeritCycleAllocationStore
{
    public static IMeritCycleAllocationStore NewMemoryMeritCycleAllocationStore()
    {
        return new MemoryMeritCycleAllocationStore(
            new Dictionary<string, MeritCycleAllocation>(),
            new List<string>());
    }

    public static IMeritCycleAllocationStore NewCachedMeritCycleAllocationStore(MemoryMeritCycleAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleAllocationStore(inner, MeritCycleAllocationCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<MeritCycleAllocation> FindMeritCycleAllocations(IMeritCycleAllocationStore store, MeritCycleAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
