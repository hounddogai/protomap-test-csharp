namespace Acme.Workforce.Compensation.Adjustment.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global adjustment bundles by code.</summary>
public interface IGlobalAdjustmentBundleStore
{
    GlobalAdjustmentBundle? Find(string code);
    void Save(GlobalAdjustmentBundle item);
    bool Remove(string code);
    List<GlobalAdjustmentBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalAdjustmentBundleStore(Dictionary<string, GlobalAdjustmentBundle> items, List<string> order) : IGlobalAdjustmentBundleStore
{
    public Dictionary<string, GlobalAdjustmentBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalAdjustmentBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalAdjustmentBundle item)
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

    public List<GlobalAdjustmentBundle> List()
    {
        var outValue = new List<GlobalAdjustmentBundle>();
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
public sealed class CachedGlobalAdjustmentBundleStore(MemoryGlobalAdjustmentBundleStore inner, GlobalAdjustmentBundleCache cache, int hits, int misses) : IGlobalAdjustmentBundleStore
{
    public MemoryGlobalAdjustmentBundleStore Inner { get; set; } = inner;
    public GlobalAdjustmentBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalAdjustmentBundle? Find(string code)
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

    public void Save(GlobalAdjustmentBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalAdjustmentBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global adjustment bundles. Empty fields match all.</summary>
public sealed record GlobalAdjustmentBundleQuery(string Name, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalAdjustmentBundle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalAdjustmentBundle> Apply(List<GlobalAdjustmentBundle> items)
    {
        var matched = Collections.Filter(items, (GlobalAdjustmentBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global adjustment bundle store.</summary>
public static class GlobalAdjustmentBundleStore
{
    public static IGlobalAdjustmentBundleStore NewMemoryGlobalAdjustmentBundleStore()
    {
        return new MemoryGlobalAdjustmentBundleStore(
            new Dictionary<string, GlobalAdjustmentBundle>(),
            new List<string>());
    }

    public static IGlobalAdjustmentBundleStore NewCachedGlobalAdjustmentBundleStore(MemoryGlobalAdjustmentBundleStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalAdjustmentBundleStore(
            inner,
            GlobalAdjustmentBundleCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<GlobalAdjustmentBundle> FindGlobalAdjustmentBundles(IGlobalAdjustmentBundleStore store, GlobalAdjustmentBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
