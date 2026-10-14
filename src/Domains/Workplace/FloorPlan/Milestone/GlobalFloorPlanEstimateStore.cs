namespace Acme.Workforce.Workplace.FloorPlan.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global floor plan estimates by code.</summary>
public interface IGlobalFloorPlanEstimateStore
{
    GlobalFloorPlanEstimate? Find(string code);
    void Save(GlobalFloorPlanEstimate item);
    bool Remove(string code);
    List<GlobalFloorPlanEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalFloorPlanEstimateStore(Dictionary<string, GlobalFloorPlanEstimate> items, List<string> order) : IGlobalFloorPlanEstimateStore
{
    public Dictionary<string, GlobalFloorPlanEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalFloorPlanEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalFloorPlanEstimate item)
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

    public List<GlobalFloorPlanEstimate> List()
    {
        var outValue = new List<GlobalFloorPlanEstimate>();
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
public sealed class CachedGlobalFloorPlanEstimateStore(MemoryGlobalFloorPlanEstimateStore inner, GlobalFloorPlanEstimateCache cache, int hits, int misses) : IGlobalFloorPlanEstimateStore
{
    public MemoryGlobalFloorPlanEstimateStore Inner { get; set; } = inner;
    public GlobalFloorPlanEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalFloorPlanEstimate? Find(string code)
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

    public void Save(GlobalFloorPlanEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalFloorPlanEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global floor plan estimates. Empty fields match all.</summary>
public sealed record GlobalFloorPlanEstimateQuery(string Source, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalFloorPlanEstimate item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalFloorPlanEstimate> Apply(List<GlobalFloorPlanEstimate> items)
    {
        var matched = Collections.Filter(items, (GlobalFloorPlanEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global floor plan estimate store.</summary>
public static class GlobalFloorPlanEstimateStore
{
    public static IGlobalFloorPlanEstimateStore NewMemoryGlobalFloorPlanEstimateStore()
    {
        return new MemoryGlobalFloorPlanEstimateStore(
            new Dictionary<string, GlobalFloorPlanEstimate>(),
            new List<string>());
    }

    public static IGlobalFloorPlanEstimateStore NewCachedGlobalFloorPlanEstimateStore(MemoryGlobalFloorPlanEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalFloorPlanEstimateStore(
            inner,
            GlobalFloorPlanEstimateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<GlobalFloorPlanEstimate> FindGlobalFloorPlanEstimates(IGlobalFloorPlanEstimateStore store, GlobalFloorPlanEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
