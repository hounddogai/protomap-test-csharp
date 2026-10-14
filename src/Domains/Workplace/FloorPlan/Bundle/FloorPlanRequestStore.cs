namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan requests by code.</summary>
public interface IFloorPlanRequestStore
{
    FloorPlanRequest? Find(string code);
    void Save(FloorPlanRequest item);
    bool Remove(string code);
    List<FloorPlanRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanRequestStore(Dictionary<string, FloorPlanRequest> items, List<string> order) : IFloorPlanRequestStore
{
    public Dictionary<string, FloorPlanRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanRequest item)
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

    public List<FloorPlanRequest> List()
    {
        var outValue = new List<FloorPlanRequest>();
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
public sealed class CachedFloorPlanRequestStore(MemoryFloorPlanRequestStore inner, FloorPlanRequestCache cache, int hits, int misses) : IFloorPlanRequestStore
{
    public MemoryFloorPlanRequestStore Inner { get; set; } = inner;
    public FloorPlanRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanRequest? Find(string code)
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

    public void Save(FloorPlanRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan requests. Empty fields match all.</summary>
public sealed record FloorPlanRequestQuery(string Team, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanRequest item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FloorPlanRequest> Apply(List<FloorPlanRequest> items)
    {
        var matched = Collections.Filter(items, (FloorPlanRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan request store.</summary>
public static class FloorPlanRequestStore
{
    public static IFloorPlanRequestStore NewMemoryFloorPlanRequestStore()
    {
        return new MemoryFloorPlanRequestStore(new Dictionary<string, FloorPlanRequest>(), new List<string>());
    }

    public static IFloorPlanRequestStore NewCachedFloorPlanRequestStore(MemoryFloorPlanRequestStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanRequestStore(inner, FloorPlanRequestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FloorPlanRequest> FindFloorPlanRequests(IFloorPlanRequestStore store, FloorPlanRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
