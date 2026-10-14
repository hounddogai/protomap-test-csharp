namespace Acme.Workforce.Workplace.FloorPlan.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback floor plan requests by code.</summary>
public interface IFallbackFloorPlanRequestStore
{
    FallbackFloorPlanRequest? Find(string code);
    void Save(FallbackFloorPlanRequest item);
    bool Remove(string code);
    List<FallbackFloorPlanRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackFloorPlanRequestStore(Dictionary<string, FallbackFloorPlanRequest> items, List<string> order) : IFallbackFloorPlanRequestStore
{
    public Dictionary<string, FallbackFloorPlanRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackFloorPlanRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackFloorPlanRequest item)
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

    public List<FallbackFloorPlanRequest> List()
    {
        var outValue = new List<FallbackFloorPlanRequest>();
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
public sealed class CachedFallbackFloorPlanRequestStore(MemoryFallbackFloorPlanRequestStore inner, FallbackFloorPlanRequestCache cache, int hits, int misses) : IFallbackFloorPlanRequestStore
{
    public MemoryFallbackFloorPlanRequestStore Inner { get; set; } = inner;
    public FallbackFloorPlanRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackFloorPlanRequest? Find(string code)
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

    public void Save(FallbackFloorPlanRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackFloorPlanRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback floor plan requests. Empty fields match all.</summary>
public sealed record FallbackFloorPlanRequestQuery(string Label, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackFloorPlanRequest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackFloorPlanRequest> Apply(List<FallbackFloorPlanRequest> items)
    {
        var matched = Collections.Filter(items, (FallbackFloorPlanRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback floor plan request store.</summary>
public static class FallbackFloorPlanRequestStore
{
    public static IFallbackFloorPlanRequestStore NewMemoryFallbackFloorPlanRequestStore()
    {
        return new MemoryFallbackFloorPlanRequestStore(
            new Dictionary<string, FallbackFloorPlanRequest>(),
            new List<string>());
    }

    public static IFallbackFloorPlanRequestStore NewCachedFallbackFloorPlanRequestStore(MemoryFallbackFloorPlanRequestStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackFloorPlanRequestStore(
            inner,
            FallbackFloorPlanRequestCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackFloorPlanRequest> FindFallbackFloorPlanRequests(IFallbackFloorPlanRequestStore store, FallbackFloorPlanRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
