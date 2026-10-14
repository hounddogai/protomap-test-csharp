namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict floor plan bundles by code.</summary>
public interface IStrictFloorPlanBundleStore
{
    StrictFloorPlanBundle? Find(string code);
    void Save(StrictFloorPlanBundle item);
    bool Remove(string code);
    List<StrictFloorPlanBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictFloorPlanBundleStore(Dictionary<string, StrictFloorPlanBundle> items, List<string> order) : IStrictFloorPlanBundleStore
{
    public Dictionary<string, StrictFloorPlanBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictFloorPlanBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictFloorPlanBundle item)
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

    public List<StrictFloorPlanBundle> List()
    {
        var outValue = new List<StrictFloorPlanBundle>();
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
public sealed class CachedStrictFloorPlanBundleStore(MemoryStrictFloorPlanBundleStore inner, StrictFloorPlanBundleCache cache, int hits, int misses) : IStrictFloorPlanBundleStore
{
    public MemoryStrictFloorPlanBundleStore Inner { get; set; } = inner;
    public StrictFloorPlanBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictFloorPlanBundle? Find(string code)
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

    public void Save(StrictFloorPlanBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictFloorPlanBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict floor plan bundles. Empty fields match all.</summary>
public sealed record StrictFloorPlanBundleQuery(string Unit, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(StrictFloorPlanBundle item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<StrictFloorPlanBundle> Apply(List<StrictFloorPlanBundle> items)
    {
        var matched = Collections.Filter(items, (StrictFloorPlanBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict floor plan bundle store.</summary>
public static class StrictFloorPlanBundleStore
{
    public static IStrictFloorPlanBundleStore NewMemoryStrictFloorPlanBundleStore()
    {
        return new MemoryStrictFloorPlanBundleStore(
            new Dictionary<string, StrictFloorPlanBundle>(),
            new List<string>());
    }

    public static IStrictFloorPlanBundleStore NewCachedStrictFloorPlanBundleStore(MemoryStrictFloorPlanBundleStore inner, int ttl, IClock clock)
    {
        return new CachedStrictFloorPlanBundleStore(inner, StrictFloorPlanBundleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<StrictFloorPlanBundle> FindStrictFloorPlanBundles(IStrictFloorPlanBundleStore store, StrictFloorPlanBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
