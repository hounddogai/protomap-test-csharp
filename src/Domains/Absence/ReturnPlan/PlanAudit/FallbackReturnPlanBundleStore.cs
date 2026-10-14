namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback return plan bundles by code.</summary>
public interface IFallbackReturnPlanBundleStore
{
    FallbackReturnPlanBundle? Find(string code);
    void Save(FallbackReturnPlanBundle item);
    bool Remove(string code);
    List<FallbackReturnPlanBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackReturnPlanBundleStore(Dictionary<string, FallbackReturnPlanBundle> items, List<string> order) : IFallbackReturnPlanBundleStore
{
    public Dictionary<string, FallbackReturnPlanBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackReturnPlanBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackReturnPlanBundle item)
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

    public List<FallbackReturnPlanBundle> List()
    {
        var outValue = new List<FallbackReturnPlanBundle>();
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
public sealed class CachedFallbackReturnPlanBundleStore(MemoryFallbackReturnPlanBundleStore inner, FallbackReturnPlanBundleCache cache, int hits, int misses) : IFallbackReturnPlanBundleStore
{
    public MemoryFallbackReturnPlanBundleStore Inner { get; set; } = inner;
    public FallbackReturnPlanBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackReturnPlanBundle? Find(string code)
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

    public void Save(FallbackReturnPlanBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackReturnPlanBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback return plan bundles. Empty fields match all.</summary>
public sealed record FallbackReturnPlanBundleQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackReturnPlanBundle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackReturnPlanBundle> Apply(List<FallbackReturnPlanBundle> items)
    {
        var matched = Collections.Filter(items, (FallbackReturnPlanBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback return plan bundle store.</summary>
public static class FallbackReturnPlanBundleStore
{
    public static IFallbackReturnPlanBundleStore NewMemoryFallbackReturnPlanBundleStore()
    {
        return new MemoryFallbackReturnPlanBundleStore(
            new Dictionary<string, FallbackReturnPlanBundle>(),
            new List<string>());
    }

    public static IFallbackReturnPlanBundleStore NewCachedFallbackReturnPlanBundleStore(MemoryFallbackReturnPlanBundleStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackReturnPlanBundleStore(
            inner,
            FallbackReturnPlanBundleCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackReturnPlanBundle> FindFallbackReturnPlanBundles(IFallbackReturnPlanBundleStore store, FallbackReturnPlanBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
