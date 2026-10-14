namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default floor plan segments by code.</summary>
public interface IDefaultFloorPlanSegmentStore
{
    DefaultFloorPlanSegment? Find(string code);
    void Save(DefaultFloorPlanSegment item);
    bool Remove(string code);
    List<DefaultFloorPlanSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultFloorPlanSegmentStore(Dictionary<string, DefaultFloorPlanSegment> items, List<string> order) : IDefaultFloorPlanSegmentStore
{
    public Dictionary<string, DefaultFloorPlanSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultFloorPlanSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultFloorPlanSegment item)
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

    public List<DefaultFloorPlanSegment> List()
    {
        var outValue = new List<DefaultFloorPlanSegment>();
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
public sealed class CachedDefaultFloorPlanSegmentStore(MemoryDefaultFloorPlanSegmentStore inner, DefaultFloorPlanSegmentCache cache, int hits, int misses) : IDefaultFloorPlanSegmentStore
{
    public MemoryDefaultFloorPlanSegmentStore Inner { get; set; } = inner;
    public DefaultFloorPlanSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultFloorPlanSegment? Find(string code)
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

    public void Save(DefaultFloorPlanSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultFloorPlanSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default floor plan segments. Empty fields match all.</summary>
public sealed record DefaultFloorPlanSegmentQuery(string Name, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultFloorPlanSegment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultFloorPlanSegment> Apply(List<DefaultFloorPlanSegment> items)
    {
        var matched = Collections.Filter(items, (DefaultFloorPlanSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default floor plan segment store.</summary>
public static class DefaultFloorPlanSegmentStore
{
    public static IDefaultFloorPlanSegmentStore NewMemoryDefaultFloorPlanSegmentStore()
    {
        return new MemoryDefaultFloorPlanSegmentStore(
            new Dictionary<string, DefaultFloorPlanSegment>(),
            new List<string>());
    }

    public static IDefaultFloorPlanSegmentStore NewCachedDefaultFloorPlanSegmentStore(MemoryDefaultFloorPlanSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultFloorPlanSegmentStore(
            inner,
            DefaultFloorPlanSegmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<DefaultFloorPlanSegment> FindDefaultFloorPlanSegments(IDefaultFloorPlanSegmentStore store, DefaultFloorPlanSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
