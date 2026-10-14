namespace Acme.Workforce.Workplace.Occupancy.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy snapshots by code.</summary>
public interface IOccupancySnapshotStore
{
    OccupancySnapshot? Find(string code);
    void Save(OccupancySnapshot item);
    bool Remove(string code);
    List<OccupancySnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancySnapshotStore(Dictionary<string, OccupancySnapshot> items, List<string> order) : IOccupancySnapshotStore
{
    public Dictionary<string, OccupancySnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancySnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancySnapshot item)
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

    public List<OccupancySnapshot> List()
    {
        var outValue = new List<OccupancySnapshot>();
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
public sealed class CachedOccupancySnapshotStore(MemoryOccupancySnapshotStore inner, OccupancySnapshotCache cache, int hits, int misses) : IOccupancySnapshotStore
{
    public MemoryOccupancySnapshotStore Inner { get; set; } = inner;
    public OccupancySnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancySnapshot? Find(string code)
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

    public void Save(OccupancySnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancySnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy snapshots. Empty fields match all.</summary>
public sealed record OccupancySnapshotQuery(string Segment, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancySnapshot item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancySnapshot> Apply(List<OccupancySnapshot> items)
    {
        var matched = Collections.Filter(items, (OccupancySnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy snapshot store.</summary>
public static class OccupancySnapshotStore
{
    public static IOccupancySnapshotStore NewMemoryOccupancySnapshotStore()
    {
        return new MemoryOccupancySnapshotStore(new Dictionary<string, OccupancySnapshot>(), new List<string>());
    }

    public static IOccupancySnapshotStore NewCachedOccupancySnapshotStore(MemoryOccupancySnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancySnapshotStore(inner, OccupancySnapshotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<OccupancySnapshot> FindOccupancySnapshots(IOccupancySnapshotStore store, OccupancySnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
