namespace Acme.Workforce.Workplace.FloorPlan.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan estimates by code.</summary>
public interface IFloorPlanEstimateStore
{
    FloorPlanEstimate? Find(string code);
    void Save(FloorPlanEstimate item);
    bool Remove(string code);
    List<FloorPlanEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanEstimateStore(Dictionary<string, FloorPlanEstimate> items, List<string> order) : IFloorPlanEstimateStore
{
    public Dictionary<string, FloorPlanEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanEstimate item)
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

    public List<FloorPlanEstimate> List()
    {
        var outValue = new List<FloorPlanEstimate>();
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
public sealed class CachedFloorPlanEstimateStore(MemoryFloorPlanEstimateStore inner, FloorPlanEstimateCache cache, int hits, int misses) : IFloorPlanEstimateStore
{
    public MemoryFloorPlanEstimateStore Inner { get; set; } = inner;
    public FloorPlanEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanEstimate? Find(string code)
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

    public void Save(FloorPlanEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan estimates. Empty fields match all.</summary>
public sealed record FloorPlanEstimateQuery(string Locale, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanEstimate item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<FloorPlanEstimate> Apply(List<FloorPlanEstimate> items)
    {
        var matched = Collections.Filter(items, (FloorPlanEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan estimate store.</summary>
public static class FloorPlanEstimateStore
{
    public static IFloorPlanEstimateStore NewMemoryFloorPlanEstimateStore()
    {
        return new MemoryFloorPlanEstimateStore(new Dictionary<string, FloorPlanEstimate>(), new List<string>());
    }

    public static IFloorPlanEstimateStore NewCachedFloorPlanEstimateStore(MemoryFloorPlanEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanEstimateStore(inner, FloorPlanEstimateCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FloorPlanEstimate> FindFloorPlanEstimates(IFloorPlanEstimateStore store, FloorPlanEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
