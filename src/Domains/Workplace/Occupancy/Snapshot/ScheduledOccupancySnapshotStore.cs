namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled occupancy snapshots by code.</summary>
public interface IScheduledOccupancySnapshotStore
{
    ScheduledOccupancySnapshot? Find(string code);
    void Save(ScheduledOccupancySnapshot item);
    bool Remove(string code);
    List<ScheduledOccupancySnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledOccupancySnapshotStore(Dictionary<string, ScheduledOccupancySnapshot> items, List<string> order) : IScheduledOccupancySnapshotStore
{
    public Dictionary<string, ScheduledOccupancySnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledOccupancySnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledOccupancySnapshot item)
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

    public List<ScheduledOccupancySnapshot> List()
    {
        var outValue = new List<ScheduledOccupancySnapshot>();
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
public sealed class CachedScheduledOccupancySnapshotStore(MemoryScheduledOccupancySnapshotStore inner, ScheduledOccupancySnapshotCache cache, int hits, int misses) : IScheduledOccupancySnapshotStore
{
    public MemoryScheduledOccupancySnapshotStore Inner { get; set; } = inner;
    public ScheduledOccupancySnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledOccupancySnapshot? Find(string code)
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

    public void Save(ScheduledOccupancySnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledOccupancySnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled occupancy snapshots. Empty fields match all.</summary>
public sealed record ScheduledOccupancySnapshotQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledOccupancySnapshot item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledOccupancySnapshot> Apply(List<ScheduledOccupancySnapshot> items)
    {
        var matched = Collections.Filter(items, (ScheduledOccupancySnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled occupancy snapshot store.</summary>
public static class ScheduledOccupancySnapshotStore
{
    public static IScheduledOccupancySnapshotStore NewMemoryScheduledOccupancySnapshotStore()
    {
        return new MemoryScheduledOccupancySnapshotStore(
            new Dictionary<string, ScheduledOccupancySnapshot>(),
            new List<string>());
    }

    public static IScheduledOccupancySnapshotStore NewCachedScheduledOccupancySnapshotStore(MemoryScheduledOccupancySnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledOccupancySnapshotStore(
            inner,
            ScheduledOccupancySnapshotCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledOccupancySnapshot> FindScheduledOccupancySnapshots(IScheduledOccupancySnapshotStore store, ScheduledOccupancySnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
