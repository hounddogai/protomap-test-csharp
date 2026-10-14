namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient action plan bundles by code.</summary>
public interface ILenientActionPlanBundleStore
{
    LenientActionPlanBundle? Find(string code);
    void Save(LenientActionPlanBundle item);
    bool Remove(string code);
    List<LenientActionPlanBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientActionPlanBundleStore(Dictionary<string, LenientActionPlanBundle> items, List<string> order) : ILenientActionPlanBundleStore
{
    public Dictionary<string, LenientActionPlanBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientActionPlanBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientActionPlanBundle item)
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

    public List<LenientActionPlanBundle> List()
    {
        var outValue = new List<LenientActionPlanBundle>();
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
public sealed class CachedLenientActionPlanBundleStore(MemoryLenientActionPlanBundleStore inner, LenientActionPlanBundleCache cache, int hits, int misses) : ILenientActionPlanBundleStore
{
    public MemoryLenientActionPlanBundleStore Inner { get; set; } = inner;
    public LenientActionPlanBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientActionPlanBundle? Find(string code)
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

    public void Save(LenientActionPlanBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientActionPlanBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient action plan bundles. Empty fields match all.</summary>
public sealed record LenientActionPlanBundleQuery(string Name, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(LenientActionPlanBundle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientActionPlanBundle> Apply(List<LenientActionPlanBundle> items)
    {
        var matched = Collections.Filter(items, (LenientActionPlanBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient action plan bundle store.</summary>
public static class LenientActionPlanBundleStore
{
    public static ILenientActionPlanBundleStore NewMemoryLenientActionPlanBundleStore()
    {
        return new MemoryLenientActionPlanBundleStore(
            new Dictionary<string, LenientActionPlanBundle>(),
            new List<string>());
    }

    public static ILenientActionPlanBundleStore NewCachedLenientActionPlanBundleStore(MemoryLenientActionPlanBundleStore inner, int ttl, IClock clock)
    {
        return new CachedLenientActionPlanBundleStore(
            inner,
            LenientActionPlanBundleCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LenientActionPlanBundle> FindLenientActionPlanBundles(ILenientActionPlanBundleStore store, LenientActionPlanBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
