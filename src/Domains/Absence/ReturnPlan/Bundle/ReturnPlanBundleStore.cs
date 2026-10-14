namespace Acme.Workforce.Absence.ReturnPlan.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan bundles by code.</summary>
public interface IReturnPlanBundleStore
{
    ReturnPlanBundle? Find(string code);
    void Save(ReturnPlanBundle item);
    bool Remove(string code);
    List<ReturnPlanBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanBundleStore(Dictionary<string, ReturnPlanBundle> items, List<string> order) : IReturnPlanBundleStore
{
    public Dictionary<string, ReturnPlanBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanBundle item)
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

    public List<ReturnPlanBundle> List()
    {
        var outValue = new List<ReturnPlanBundle>();
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
public sealed class CachedReturnPlanBundleStore(MemoryReturnPlanBundleStore inner, ReturnPlanBundleCache cache, int hits, int misses) : IReturnPlanBundleStore
{
    public MemoryReturnPlanBundleStore Inner { get; set; } = inner;
    public ReturnPlanBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanBundle? Find(string code)
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

    public void Save(ReturnPlanBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan bundles. Empty fields match all.</summary>
public sealed record ReturnPlanBundleQuery(string Source, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanBundle item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<ReturnPlanBundle> Apply(List<ReturnPlanBundle> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan bundle store.</summary>
public static class ReturnPlanBundleStore
{
    public static IReturnPlanBundleStore NewMemoryReturnPlanBundleStore()
    {
        return new MemoryReturnPlanBundleStore(new Dictionary<string, ReturnPlanBundle>(), new List<string>());
    }

    public static IReturnPlanBundleStore NewCachedReturnPlanBundleStore(MemoryReturnPlanBundleStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanBundleStore(inner, ReturnPlanBundleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanBundle> FindReturnPlanBundles(IReturnPlanBundleStore store, ReturnPlanBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
