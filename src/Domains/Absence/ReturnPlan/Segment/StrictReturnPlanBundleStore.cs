namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict return plan bundles by code.</summary>
public interface IStrictReturnPlanBundleStore
{
    StrictReturnPlanBundle? Find(string code);
    void Save(StrictReturnPlanBundle item);
    bool Remove(string code);
    List<StrictReturnPlanBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictReturnPlanBundleStore(Dictionary<string, StrictReturnPlanBundle> items, List<string> order) : IStrictReturnPlanBundleStore
{
    public Dictionary<string, StrictReturnPlanBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictReturnPlanBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictReturnPlanBundle item)
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

    public List<StrictReturnPlanBundle> List()
    {
        var outValue = new List<StrictReturnPlanBundle>();
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
public sealed class CachedStrictReturnPlanBundleStore(MemoryStrictReturnPlanBundleStore inner, StrictReturnPlanBundleCache cache, int hits, int misses) : IStrictReturnPlanBundleStore
{
    public MemoryStrictReturnPlanBundleStore Inner { get; set; } = inner;
    public StrictReturnPlanBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictReturnPlanBundle? Find(string code)
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

    public void Save(StrictReturnPlanBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictReturnPlanBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict return plan bundles. Empty fields match all.</summary>
public sealed record StrictReturnPlanBundleQuery(string Source, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(StrictReturnPlanBundle item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictReturnPlanBundle> Apply(List<StrictReturnPlanBundle> items)
    {
        var matched = Collections.Filter(items, (StrictReturnPlanBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict return plan bundle store.</summary>
public static class StrictReturnPlanBundleStore
{
    public static IStrictReturnPlanBundleStore NewMemoryStrictReturnPlanBundleStore()
    {
        return new MemoryStrictReturnPlanBundleStore(
            new Dictionary<string, StrictReturnPlanBundle>(),
            new List<string>());
    }

    public static IStrictReturnPlanBundleStore NewCachedStrictReturnPlanBundleStore(MemoryStrictReturnPlanBundleStore inner, int ttl, IClock clock)
    {
        return new CachedStrictReturnPlanBundleStore(
            inner,
            StrictReturnPlanBundleCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<StrictReturnPlanBundle> FindStrictReturnPlanBundles(IStrictReturnPlanBundleStore store, StrictReturnPlanBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
