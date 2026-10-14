namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict occupancy segments by code.</summary>
public interface IStrictOccupancySegmentStore
{
    StrictOccupancySegment? Find(string code);
    void Save(StrictOccupancySegment item);
    bool Remove(string code);
    List<StrictOccupancySegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictOccupancySegmentStore(Dictionary<string, StrictOccupancySegment> items, List<string> order) : IStrictOccupancySegmentStore
{
    public Dictionary<string, StrictOccupancySegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictOccupancySegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictOccupancySegment item)
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

    public List<StrictOccupancySegment> List()
    {
        var outValue = new List<StrictOccupancySegment>();
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
public sealed class CachedStrictOccupancySegmentStore(MemoryStrictOccupancySegmentStore inner, StrictOccupancySegmentCache cache, int hits, int misses) : IStrictOccupancySegmentStore
{
    public MemoryStrictOccupancySegmentStore Inner { get; set; } = inner;
    public StrictOccupancySegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictOccupancySegment? Find(string code)
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

    public void Save(StrictOccupancySegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictOccupancySegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict occupancy segments. Empty fields match all.</summary>
public sealed record StrictOccupancySegmentQuery(string Region, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(StrictOccupancySegment item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictOccupancySegment> Apply(List<StrictOccupancySegment> items)
    {
        var matched = Collections.Filter(items, (StrictOccupancySegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict occupancy segment store.</summary>
public static class StrictOccupancySegmentStore
{
    public static IStrictOccupancySegmentStore NewMemoryStrictOccupancySegmentStore()
    {
        return new MemoryStrictOccupancySegmentStore(
            new Dictionary<string, StrictOccupancySegment>(),
            new List<string>());
    }

    public static IStrictOccupancySegmentStore NewCachedStrictOccupancySegmentStore(MemoryStrictOccupancySegmentStore inner, int ttl, IClock clock)
    {
        return new CachedStrictOccupancySegmentStore(
            inner,
            StrictOccupancySegmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StrictOccupancySegment> FindStrictOccupancySegments(IStrictOccupancySegmentStore store, StrictOccupancySegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
