namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled floor plan snapshots by code.</summary>
public interface IScheduledFloorPlanSnapshotStore
{
    ScheduledFloorPlanSnapshot? Find(string code);
    void Save(ScheduledFloorPlanSnapshot item);
    bool Remove(string code);
    List<ScheduledFloorPlanSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledFloorPlanSnapshotStore(Dictionary<string, ScheduledFloorPlanSnapshot> items, List<string> order) : IScheduledFloorPlanSnapshotStore
{
    public Dictionary<string, ScheduledFloorPlanSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledFloorPlanSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledFloorPlanSnapshot item)
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

    public List<ScheduledFloorPlanSnapshot> List()
    {
        var outValue = new List<ScheduledFloorPlanSnapshot>();
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
public sealed class CachedScheduledFloorPlanSnapshotStore(MemoryScheduledFloorPlanSnapshotStore inner, ScheduledFloorPlanSnapshotCache cache, int hits, int misses) : IScheduledFloorPlanSnapshotStore
{
    public MemoryScheduledFloorPlanSnapshotStore Inner { get; set; } = inner;
    public ScheduledFloorPlanSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledFloorPlanSnapshot? Find(string code)
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

    public void Save(ScheduledFloorPlanSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledFloorPlanSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled floor plan snapshots. Empty fields match all.</summary>
public sealed record ScheduledFloorPlanSnapshotQuery(string Title, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledFloorPlanSnapshot item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<ScheduledFloorPlanSnapshot> Apply(List<ScheduledFloorPlanSnapshot> items)
    {
        var matched = Collections.Filter(items, (ScheduledFloorPlanSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled floor plan snapshot store.</summary>
public static class ScheduledFloorPlanSnapshotStore
{
    public static IScheduledFloorPlanSnapshotStore NewMemoryScheduledFloorPlanSnapshotStore()
    {
        return new MemoryScheduledFloorPlanSnapshotStore(
            new Dictionary<string, ScheduledFloorPlanSnapshot>(),
            new List<string>());
    }

    public static IScheduledFloorPlanSnapshotStore NewCachedScheduledFloorPlanSnapshotStore(MemoryScheduledFloorPlanSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledFloorPlanSnapshotStore(
            inner,
            ScheduledFloorPlanSnapshotCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledFloorPlanSnapshot> FindScheduledFloorPlanSnapshots(IScheduledFloorPlanSnapshotStore store, ScheduledFloorPlanSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
