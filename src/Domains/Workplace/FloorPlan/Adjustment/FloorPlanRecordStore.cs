namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan records by code.</summary>
public interface IFloorPlanRecordStore
{
    FloorPlanRecord? Find(string code);
    void Save(FloorPlanRecord item);
    bool Remove(string code);
    List<FloorPlanRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanRecordStore(Dictionary<string, FloorPlanRecord> items, List<string> order) : IFloorPlanRecordStore
{
    public Dictionary<string, FloorPlanRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanRecord item)
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

    public List<FloorPlanRecord> List()
    {
        var outValue = new List<FloorPlanRecord>();
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
public sealed class CachedFloorPlanRecordStore(MemoryFloorPlanRecordStore inner, FloorPlanRecordCache cache, int hits, int misses) : IFloorPlanRecordStore
{
    public MemoryFloorPlanRecordStore Inner { get; set; } = inner;
    public FloorPlanRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanRecord? Find(string code)
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

    public void Save(FloorPlanRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan records. Empty fields match all.</summary>
public sealed record FloorPlanRecordQuery(string Source, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanRecord item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FloorPlanRecord> Apply(List<FloorPlanRecord> items)
    {
        var matched = Collections.Filter(items, (FloorPlanRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan record store.</summary>
public static class FloorPlanRecordStore
{
    public static IFloorPlanRecordStore NewMemoryFloorPlanRecordStore()
    {
        return new MemoryFloorPlanRecordStore(new Dictionary<string, FloorPlanRecord>(), new List<string>());
    }

    public static IFloorPlanRecordStore NewCachedFloorPlanRecordStore(MemoryFloorPlanRecordStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanRecordStore(inner, FloorPlanRecordCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FloorPlanRecord> FindFloorPlanRecords(IFloorPlanRecordStore store, FloorPlanRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
