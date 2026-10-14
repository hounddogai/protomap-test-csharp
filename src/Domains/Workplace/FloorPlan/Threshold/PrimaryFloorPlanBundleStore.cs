namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary floor plan bundles by code.</summary>
public interface IPrimaryFloorPlanBundleStore
{
    PrimaryFloorPlanBundle? Find(string code);
    void Save(PrimaryFloorPlanBundle item);
    bool Remove(string code);
    List<PrimaryFloorPlanBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryFloorPlanBundleStore(Dictionary<string, PrimaryFloorPlanBundle> items, List<string> order) : IPrimaryFloorPlanBundleStore
{
    public Dictionary<string, PrimaryFloorPlanBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryFloorPlanBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryFloorPlanBundle item)
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

    public List<PrimaryFloorPlanBundle> List()
    {
        var outValue = new List<PrimaryFloorPlanBundle>();
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
public sealed class CachedPrimaryFloorPlanBundleStore(MemoryPrimaryFloorPlanBundleStore inner, PrimaryFloorPlanBundleCache cache, int hits, int misses) : IPrimaryFloorPlanBundleStore
{
    public MemoryPrimaryFloorPlanBundleStore Inner { get; set; } = inner;
    public PrimaryFloorPlanBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryFloorPlanBundle? Find(string code)
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

    public void Save(PrimaryFloorPlanBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryFloorPlanBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary floor plan bundles. Empty fields match all.</summary>
public sealed record PrimaryFloorPlanBundleQuery(string Name, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryFloorPlanBundle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryFloorPlanBundle> Apply(List<PrimaryFloorPlanBundle> items)
    {
        var matched = Collections.Filter(items, (PrimaryFloorPlanBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary floor plan bundle store.</summary>
public static class PrimaryFloorPlanBundleStore
{
    public static IPrimaryFloorPlanBundleStore NewMemoryPrimaryFloorPlanBundleStore()
    {
        return new MemoryPrimaryFloorPlanBundleStore(
            new Dictionary<string, PrimaryFloorPlanBundle>(),
            new List<string>());
    }

    public static IPrimaryFloorPlanBundleStore NewCachedPrimaryFloorPlanBundleStore(MemoryPrimaryFloorPlanBundleStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryFloorPlanBundleStore(
            inner,
            PrimaryFloorPlanBundleCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryFloorPlanBundle> FindPrimaryFloorPlanBundles(IPrimaryFloorPlanBundleStore store, PrimaryFloorPlanBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
