namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary occupancy segments by code.</summary>
public interface IPrimaryOccupancySegmentStore
{
    PrimaryOccupancySegment? Find(string code);
    void Save(PrimaryOccupancySegment item);
    bool Remove(string code);
    List<PrimaryOccupancySegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryOccupancySegmentStore(Dictionary<string, PrimaryOccupancySegment> items, List<string> order) : IPrimaryOccupancySegmentStore
{
    public Dictionary<string, PrimaryOccupancySegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryOccupancySegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryOccupancySegment item)
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

    public List<PrimaryOccupancySegment> List()
    {
        var outValue = new List<PrimaryOccupancySegment>();
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
public sealed class CachedPrimaryOccupancySegmentStore(MemoryPrimaryOccupancySegmentStore inner, PrimaryOccupancySegmentCache cache, int hits, int misses) : IPrimaryOccupancySegmentStore
{
    public MemoryPrimaryOccupancySegmentStore Inner { get; set; } = inner;
    public PrimaryOccupancySegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryOccupancySegment? Find(string code)
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

    public void Save(PrimaryOccupancySegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryOccupancySegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary occupancy segments. Empty fields match all.</summary>
public sealed record PrimaryOccupancySegmentQuery(string Country, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryOccupancySegment item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryOccupancySegment> Apply(List<PrimaryOccupancySegment> items)
    {
        var matched = Collections.Filter(items, (PrimaryOccupancySegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary occupancy segment store.</summary>
public static class PrimaryOccupancySegmentStore
{
    public static IPrimaryOccupancySegmentStore NewMemoryPrimaryOccupancySegmentStore()
    {
        return new MemoryPrimaryOccupancySegmentStore(
            new Dictionary<string, PrimaryOccupancySegment>(),
            new List<string>());
    }

    public static IPrimaryOccupancySegmentStore NewCachedPrimaryOccupancySegmentStore(MemoryPrimaryOccupancySegmentStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryOccupancySegmentStore(
            inner,
            PrimaryOccupancySegmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryOccupancySegment> FindPrimaryOccupancySegments(IPrimaryOccupancySegmentStore store, PrimaryOccupancySegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
