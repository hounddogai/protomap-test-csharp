namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan checkpoints by code.</summary>
public interface IFloorPlanCheckpointStore
{
    FloorPlanCheckpoint? Find(string code);
    void Save(FloorPlanCheckpoint item);
    bool Remove(string code);
    List<FloorPlanCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanCheckpointStore(Dictionary<string, FloorPlanCheckpoint> items, List<string> order) : IFloorPlanCheckpointStore
{
    public Dictionary<string, FloorPlanCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanCheckpoint item)
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

    public List<FloorPlanCheckpoint> List()
    {
        var outValue = new List<FloorPlanCheckpoint>();
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
public sealed class CachedFloorPlanCheckpointStore(MemoryFloorPlanCheckpointStore inner, FloorPlanCheckpointCache cache, int hits, int misses) : IFloorPlanCheckpointStore
{
    public MemoryFloorPlanCheckpointStore Inner { get; set; } = inner;
    public FloorPlanCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanCheckpoint? Find(string code)
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

    public void Save(FloorPlanCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan checkpoints. Empty fields match all.</summary>
public sealed record FloorPlanCheckpointQuery(string Segment, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanCheckpoint item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FloorPlanCheckpoint> Apply(List<FloorPlanCheckpoint> items)
    {
        var matched = Collections.Filter(items, (FloorPlanCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan checkpoint store.</summary>
public static class FloorPlanCheckpointStore
{
    public static IFloorPlanCheckpointStore NewMemoryFloorPlanCheckpointStore()
    {
        return new MemoryFloorPlanCheckpointStore(new Dictionary<string, FloorPlanCheckpoint>(), new List<string>());
    }

    public static IFloorPlanCheckpointStore NewCachedFloorPlanCheckpointStore(MemoryFloorPlanCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanCheckpointStore(inner, FloorPlanCheckpointCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FloorPlanCheckpoint> FindFloorPlanCheckpoints(IFloorPlanCheckpointStore store, FloorPlanCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
