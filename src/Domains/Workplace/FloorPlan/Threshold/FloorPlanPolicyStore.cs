namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan policies by code.</summary>
public interface IFloorPlanPolicyStore
{
    FloorPlanPolicy? Find(string code);
    void Save(FloorPlanPolicy item);
    bool Remove(string code);
    List<FloorPlanPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanPolicyStore(Dictionary<string, FloorPlanPolicy> items, List<string> order) : IFloorPlanPolicyStore
{
    public Dictionary<string, FloorPlanPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanPolicy item)
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

    public List<FloorPlanPolicy> List()
    {
        var outValue = new List<FloorPlanPolicy>();
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
public sealed class CachedFloorPlanPolicyStore(MemoryFloorPlanPolicyStore inner, FloorPlanPolicyCache cache, int hits, int misses) : IFloorPlanPolicyStore
{
    public MemoryFloorPlanPolicyStore Inner { get; set; } = inner;
    public FloorPlanPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanPolicy? Find(string code)
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

    public void Save(FloorPlanPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan policies. Empty fields match all.</summary>
public sealed record FloorPlanPolicyQuery(string Title, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanPolicy item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<FloorPlanPolicy> Apply(List<FloorPlanPolicy> items)
    {
        var matched = Collections.Filter(items, (FloorPlanPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan policy store.</summary>
public static class FloorPlanPolicyStore
{
    public static IFloorPlanPolicyStore NewMemoryFloorPlanPolicyStore()
    {
        return new MemoryFloorPlanPolicyStore(new Dictionary<string, FloorPlanPolicy>(), new List<string>());
    }

    public static IFloorPlanPolicyStore NewCachedFloorPlanPolicyStore(MemoryFloorPlanPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanPolicyStore(inner, FloorPlanPolicyCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<FloorPlanPolicy> FindFloorPlanPolicies(IFloorPlanPolicyStore store, FloorPlanPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
