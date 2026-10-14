namespace Acme.Workforce.Absence.Accrual.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive accrual bundles by code.</summary>
public interface IAdaptiveAccrualBundleStore
{
    AdaptiveAccrualBundle? Find(string code);
    void Save(AdaptiveAccrualBundle item);
    bool Remove(string code);
    List<AdaptiveAccrualBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveAccrualBundleStore(Dictionary<string, AdaptiveAccrualBundle> items, List<string> order) : IAdaptiveAccrualBundleStore
{
    public Dictionary<string, AdaptiveAccrualBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveAccrualBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveAccrualBundle item)
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

    public List<AdaptiveAccrualBundle> List()
    {
        var outValue = new List<AdaptiveAccrualBundle>();
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
public sealed class CachedAdaptiveAccrualBundleStore(MemoryAdaptiveAccrualBundleStore inner, AdaptiveAccrualBundleCache cache, int hits, int misses) : IAdaptiveAccrualBundleStore
{
    public MemoryAdaptiveAccrualBundleStore Inner { get; set; } = inner;
    public AdaptiveAccrualBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveAccrualBundle? Find(string code)
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

    public void Save(AdaptiveAccrualBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveAccrualBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive accrual bundles. Empty fields match all.</summary>
public sealed record AdaptiveAccrualBundleQuery(string Label, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveAccrualBundle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveAccrualBundle> Apply(List<AdaptiveAccrualBundle> items)
    {
        var matched = Collections.Filter(items, (AdaptiveAccrualBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive accrual bundle store.</summary>
public static class AdaptiveAccrualBundleStore
{
    public static IAdaptiveAccrualBundleStore NewMemoryAdaptiveAccrualBundleStore()
    {
        return new MemoryAdaptiveAccrualBundleStore(
            new Dictionary<string, AdaptiveAccrualBundle>(),
            new List<string>());
    }

    public static IAdaptiveAccrualBundleStore NewCachedAdaptiveAccrualBundleStore(MemoryAdaptiveAccrualBundleStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveAccrualBundleStore(
            inner,
            AdaptiveAccrualBundleCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveAccrualBundle> FindAdaptiveAccrualBundles(IAdaptiveAccrualBundleStore store, AdaptiveAccrualBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
