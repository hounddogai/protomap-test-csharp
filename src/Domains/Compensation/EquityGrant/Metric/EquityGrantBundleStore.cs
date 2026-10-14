namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant bundles by code.</summary>
public interface IEquityGrantBundleStore
{
    EquityGrantBundle? Find(string code);
    void Save(EquityGrantBundle item);
    bool Remove(string code);
    List<EquityGrantBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantBundleStore(Dictionary<string, EquityGrantBundle> items, List<string> order) : IEquityGrantBundleStore
{
    public Dictionary<string, EquityGrantBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantBundle item)
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

    public List<EquityGrantBundle> List()
    {
        var outValue = new List<EquityGrantBundle>();
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
public sealed class CachedEquityGrantBundleStore(MemoryEquityGrantBundleStore inner, EquityGrantBundleCache cache, int hits, int misses) : IEquityGrantBundleStore
{
    public MemoryEquityGrantBundleStore Inner { get; set; } = inner;
    public EquityGrantBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantBundle? Find(string code)
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

    public void Save(EquityGrantBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant bundles. Empty fields match all.</summary>
public sealed record EquityGrantBundleQuery(string Label, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantBundle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantBundle> Apply(List<EquityGrantBundle> items)
    {
        var matched = Collections.Filter(items, (EquityGrantBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant bundle store.</summary>
public static class EquityGrantBundleStore
{
    public static IEquityGrantBundleStore NewMemoryEquityGrantBundleStore()
    {
        return new MemoryEquityGrantBundleStore(new Dictionary<string, EquityGrantBundle>(), new List<string>());
    }

    public static IEquityGrantBundleStore NewCachedEquityGrantBundleStore(MemoryEquityGrantBundleStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantBundleStore(inner, EquityGrantBundleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<EquityGrantBundle> FindEquityGrantBundles(IEquityGrantBundleStore store, EquityGrantBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
