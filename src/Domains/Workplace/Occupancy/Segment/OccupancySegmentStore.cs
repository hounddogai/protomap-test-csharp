namespace Acme.Workforce.Workplace.Occupancy.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy segments by code.</summary>
public interface IOccupancySegmentStore
{
    OccupancySegment? Find(string code);
    void Save(OccupancySegment item);
    bool Remove(string code);
    List<OccupancySegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancySegmentStore(Dictionary<string, OccupancySegment> items, List<string> order) : IOccupancySegmentStore
{
    public Dictionary<string, OccupancySegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancySegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancySegment item)
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

    public List<OccupancySegment> List()
    {
        var outValue = new List<OccupancySegment>();
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
public sealed class CachedOccupancySegmentStore(MemoryOccupancySegmentStore inner, OccupancySegmentCache cache, int hits, int misses) : IOccupancySegmentStore
{
    public MemoryOccupancySegmentStore Inner { get; set; } = inner;
    public OccupancySegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancySegment? Find(string code)
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

    public void Save(OccupancySegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancySegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy segments. Empty fields match all.</summary>
public sealed record OccupancySegmentQuery(string Label, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancySegment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancySegment> Apply(List<OccupancySegment> items)
    {
        var matched = Collections.Filter(items, (OccupancySegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy segment store.</summary>
public static class OccupancySegmentStore
{
    public static IOccupancySegmentStore NewMemoryOccupancySegmentStore()
    {
        return new MemoryOccupancySegmentStore(new Dictionary<string, OccupancySegment>(), new List<string>());
    }

    public static IOccupancySegmentStore NewCachedOccupancySegmentStore(MemoryOccupancySegmentStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancySegmentStore(inner, OccupancySegmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<OccupancySegment> FindOccupancySegments(IOccupancySegmentStore store, OccupancySegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
