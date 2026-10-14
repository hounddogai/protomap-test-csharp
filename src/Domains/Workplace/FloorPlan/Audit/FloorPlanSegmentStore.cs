namespace Acme.Workforce.Workplace.FloorPlan.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan segments by code.</summary>
public interface IFloorPlanSegmentStore
{
    FloorPlanSegment? Find(string code);
    void Save(FloorPlanSegment item);
    bool Remove(string code);
    List<FloorPlanSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanSegmentStore(Dictionary<string, FloorPlanSegment> items, List<string> order) : IFloorPlanSegmentStore
{
    public Dictionary<string, FloorPlanSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanSegment item)
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

    public List<FloorPlanSegment> List()
    {
        var outValue = new List<FloorPlanSegment>();
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
public sealed class CachedFloorPlanSegmentStore(MemoryFloorPlanSegmentStore inner, FloorPlanSegmentCache cache, int hits, int misses) : IFloorPlanSegmentStore
{
    public MemoryFloorPlanSegmentStore Inner { get; set; } = inner;
    public FloorPlanSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanSegment? Find(string code)
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

    public void Save(FloorPlanSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan segments. Empty fields match all.</summary>
public sealed record FloorPlanSegmentQuery(string Title, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanSegment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FloorPlanSegment> Apply(List<FloorPlanSegment> items)
    {
        var matched = Collections.Filter(items, (FloorPlanSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan segment store.</summary>
public static class FloorPlanSegmentStore
{
    public static IFloorPlanSegmentStore NewMemoryFloorPlanSegmentStore()
    {
        return new MemoryFloorPlanSegmentStore(new Dictionary<string, FloorPlanSegment>(), new List<string>());
    }

    public static IFloorPlanSegmentStore NewCachedFloorPlanSegmentStore(MemoryFloorPlanSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanSegmentStore(inner, FloorPlanSegmentCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FloorPlanSegment> FindFloorPlanSegments(IFloorPlanSegmentStore store, FloorPlanSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
