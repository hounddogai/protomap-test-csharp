namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive floor plan adjustments by code.</summary>
public interface IAdaptiveFloorPlanAdjustmentStore
{
    AdaptiveFloorPlanAdjustment? Find(string code);
    void Save(AdaptiveFloorPlanAdjustment item);
    bool Remove(string code);
    List<AdaptiveFloorPlanAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveFloorPlanAdjustmentStore(Dictionary<string, AdaptiveFloorPlanAdjustment> items, List<string> order) : IAdaptiveFloorPlanAdjustmentStore
{
    public Dictionary<string, AdaptiveFloorPlanAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveFloorPlanAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveFloorPlanAdjustment item)
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

    public List<AdaptiveFloorPlanAdjustment> List()
    {
        var outValue = new List<AdaptiveFloorPlanAdjustment>();
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
public sealed class CachedAdaptiveFloorPlanAdjustmentStore(MemoryAdaptiveFloorPlanAdjustmentStore inner, AdaptiveFloorPlanAdjustmentCache cache, int hits, int misses) : IAdaptiveFloorPlanAdjustmentStore
{
    public MemoryAdaptiveFloorPlanAdjustmentStore Inner { get; set; } = inner;
    public AdaptiveFloorPlanAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveFloorPlanAdjustment? Find(string code)
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

    public void Save(AdaptiveFloorPlanAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveFloorPlanAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive floor plan adjustments. Empty fields match all.</summary>
public sealed record AdaptiveFloorPlanAdjustmentQuery(string Department, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveFloorPlanAdjustment item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveFloorPlanAdjustment> Apply(List<AdaptiveFloorPlanAdjustment> items)
    {
        var matched = Collections.Filter(items, (AdaptiveFloorPlanAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive floor plan adjustment store.</summary>
public static class AdaptiveFloorPlanAdjustmentStore
{
    public static IAdaptiveFloorPlanAdjustmentStore NewMemoryAdaptiveFloorPlanAdjustmentStore()
    {
        return new MemoryAdaptiveFloorPlanAdjustmentStore(
            new Dictionary<string, AdaptiveFloorPlanAdjustment>(),
            new List<string>());
    }

    public static IAdaptiveFloorPlanAdjustmentStore NewCachedAdaptiveFloorPlanAdjustmentStore(MemoryAdaptiveFloorPlanAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveFloorPlanAdjustmentStore(
            inner,
            AdaptiveFloorPlanAdjustmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveFloorPlanAdjustment> FindAdaptiveFloorPlanAdjustments(IAdaptiveFloorPlanAdjustmentStore store, AdaptiveFloorPlanAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
