namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan quotas by code.</summary>
public interface IFloorPlanQuotaStore
{
    FloorPlanQuota? Find(string code);
    void Save(FloorPlanQuota item);
    bool Remove(string code);
    List<FloorPlanQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanQuotaStore(Dictionary<string, FloorPlanQuota> items, List<string> order) : IFloorPlanQuotaStore
{
    public Dictionary<string, FloorPlanQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanQuota item)
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

    public List<FloorPlanQuota> List()
    {
        var outValue = new List<FloorPlanQuota>();
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
public sealed class CachedFloorPlanQuotaStore(MemoryFloorPlanQuotaStore inner, FloorPlanQuotaCache cache, int hits, int misses) : IFloorPlanQuotaStore
{
    public MemoryFloorPlanQuotaStore Inner { get; set; } = inner;
    public FloorPlanQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanQuota? Find(string code)
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

    public void Save(FloorPlanQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan quotas. Empty fields match all.</summary>
public sealed record FloorPlanQuotaQuery(string Name, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanQuota item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FloorPlanQuota> Apply(List<FloorPlanQuota> items)
    {
        var matched = Collections.Filter(items, (FloorPlanQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan quota store.</summary>
public static class FloorPlanQuotaStore
{
    public static IFloorPlanQuotaStore NewMemoryFloorPlanQuotaStore()
    {
        return new MemoryFloorPlanQuotaStore(new Dictionary<string, FloorPlanQuota>(), new List<string>());
    }

    public static IFloorPlanQuotaStore NewCachedFloorPlanQuotaStore(MemoryFloorPlanQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanQuotaStore(inner, FloorPlanQuotaCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<FloorPlanQuota> FindFloorPlanQuotas(IFloorPlanQuotaStore store, FloorPlanQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
