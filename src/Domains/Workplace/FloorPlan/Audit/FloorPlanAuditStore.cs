namespace Acme.Workforce.Workplace.FloorPlan.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan audits by code.</summary>
public interface IFloorPlanAuditStore
{
    FloorPlanAudit? Find(string code);
    void Save(FloorPlanAudit item);
    bool Remove(string code);
    List<FloorPlanAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanAuditStore(Dictionary<string, FloorPlanAudit> items, List<string> order) : IFloorPlanAuditStore
{
    public Dictionary<string, FloorPlanAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanAudit item)
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

    public List<FloorPlanAudit> List()
    {
        var outValue = new List<FloorPlanAudit>();
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
public sealed class CachedFloorPlanAuditStore(MemoryFloorPlanAuditStore inner, FloorPlanAuditCache cache, int hits, int misses) : IFloorPlanAuditStore
{
    public MemoryFloorPlanAuditStore Inner { get; set; } = inner;
    public FloorPlanAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanAudit? Find(string code)
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

    public void Save(FloorPlanAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan audits. Empty fields match all.</summary>
public sealed record FloorPlanAuditQuery(string Region, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanAudit item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FloorPlanAudit> Apply(List<FloorPlanAudit> items)
    {
        var matched = Collections.Filter(items, (FloorPlanAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan audit store.</summary>
public static class FloorPlanAuditStore
{
    public static IFloorPlanAuditStore NewMemoryFloorPlanAuditStore()
    {
        return new MemoryFloorPlanAuditStore(new Dictionary<string, FloorPlanAudit>(), new List<string>());
    }

    public static IFloorPlanAuditStore NewCachedFloorPlanAuditStore(MemoryFloorPlanAuditStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanAuditStore(inner, FloorPlanAuditCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<FloorPlanAudit> FindFloorPlanAudits(IFloorPlanAuditStore store, FloorPlanAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
