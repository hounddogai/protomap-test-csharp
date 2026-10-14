namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan bundles by code.</summary>
public interface IFloorPlanBundleStore
{
    FloorPlanBundle? Find(string code);
    void Save(FloorPlanBundle item);
    bool Remove(string code);
    List<FloorPlanBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanBundleStore(Dictionary<string, FloorPlanBundle> items, List<string> order) : IFloorPlanBundleStore
{
    public Dictionary<string, FloorPlanBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanBundle item)
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

    public List<FloorPlanBundle> List()
    {
        var outValue = new List<FloorPlanBundle>();
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
public sealed class CachedFloorPlanBundleStore(MemoryFloorPlanBundleStore inner, FloorPlanBundleCache cache, int hits, int misses) : IFloorPlanBundleStore
{
    public MemoryFloorPlanBundleStore Inner { get; set; } = inner;
    public FloorPlanBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanBundle? Find(string code)
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

    public void Save(FloorPlanBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan bundles. Empty fields match all.</summary>
public sealed record FloorPlanBundleQuery(string Unit, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanBundle item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<FloorPlanBundle> Apply(List<FloorPlanBundle> items)
    {
        var matched = Collections.Filter(items, (FloorPlanBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan bundle store.</summary>
public static class FloorPlanBundleStore
{
    public static IFloorPlanBundleStore NewMemoryFloorPlanBundleStore()
    {
        return new MemoryFloorPlanBundleStore(new Dictionary<string, FloorPlanBundle>(), new List<string>());
    }

    public static IFloorPlanBundleStore NewCachedFloorPlanBundleStore(MemoryFloorPlanBundleStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanBundleStore(inner, FloorPlanBundleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<FloorPlanBundle> FindFloorPlanBundles(IFloorPlanBundleStore store, FloorPlanBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
