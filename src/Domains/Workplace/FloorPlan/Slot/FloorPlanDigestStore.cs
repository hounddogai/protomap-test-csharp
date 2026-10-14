namespace Acme.Workforce.Workplace.FloorPlan.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan digests by code.</summary>
public interface IFloorPlanDigestStore
{
    FloorPlanDigest? Find(string code);
    void Save(FloorPlanDigest item);
    bool Remove(string code);
    List<FloorPlanDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanDigestStore(Dictionary<string, FloorPlanDigest> items, List<string> order) : IFloorPlanDigestStore
{
    public Dictionary<string, FloorPlanDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanDigest item)
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

    public List<FloorPlanDigest> List()
    {
        var outValue = new List<FloorPlanDigest>();
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
public sealed class CachedFloorPlanDigestStore(MemoryFloorPlanDigestStore inner, FloorPlanDigestCache cache, int hits, int misses) : IFloorPlanDigestStore
{
    public MemoryFloorPlanDigestStore Inner { get; set; } = inner;
    public FloorPlanDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanDigest? Find(string code)
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

    public void Save(FloorPlanDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan digests. Empty fields match all.</summary>
public sealed record FloorPlanDigestQuery(string Unit, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanDigest item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FloorPlanDigest> Apply(List<FloorPlanDigest> items)
    {
        var matched = Collections.Filter(items, (FloorPlanDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan digest store.</summary>
public static class FloorPlanDigestStore
{
    public static IFloorPlanDigestStore NewMemoryFloorPlanDigestStore()
    {
        return new MemoryFloorPlanDigestStore(new Dictionary<string, FloorPlanDigest>(), new List<string>());
    }

    public static IFloorPlanDigestStore NewCachedFloorPlanDigestStore(MemoryFloorPlanDigestStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanDigestStore(inner, FloorPlanDigestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FloorPlanDigest> FindFloorPlanDigests(IFloorPlanDigestStore store, FloorPlanDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
