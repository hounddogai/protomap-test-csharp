namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan entries by code.</summary>
public interface IFloorPlanEntryStore
{
    FloorPlanEntry? Find(string code);
    void Save(FloorPlanEntry item);
    bool Remove(string code);
    List<FloorPlanEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanEntryStore(Dictionary<string, FloorPlanEntry> items, List<string> order) : IFloorPlanEntryStore
{
    public Dictionary<string, FloorPlanEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanEntry item)
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

    public List<FloorPlanEntry> List()
    {
        var outValue = new List<FloorPlanEntry>();
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
public sealed class CachedFloorPlanEntryStore(MemoryFloorPlanEntryStore inner, FloorPlanEntryCache cache, int hits, int misses) : IFloorPlanEntryStore
{
    public MemoryFloorPlanEntryStore Inner { get; set; } = inner;
    public FloorPlanEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanEntry? Find(string code)
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

    public void Save(FloorPlanEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan entries. Empty fields match all.</summary>
public sealed record FloorPlanEntryQuery(string Unit, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanEntry item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FloorPlanEntry> Apply(List<FloorPlanEntry> items)
    {
        var matched = Collections.Filter(items, (FloorPlanEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan entry store.</summary>
public static class FloorPlanEntryStore
{
    public static IFloorPlanEntryStore NewMemoryFloorPlanEntryStore()
    {
        return new MemoryFloorPlanEntryStore(new Dictionary<string, FloorPlanEntry>(), new List<string>());
    }

    public static IFloorPlanEntryStore NewCachedFloorPlanEntryStore(MemoryFloorPlanEntryStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanEntryStore(inner, FloorPlanEntryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<FloorPlanEntry> FindFloorPlanEntries(IFloorPlanEntryStore store, FloorPlanEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
