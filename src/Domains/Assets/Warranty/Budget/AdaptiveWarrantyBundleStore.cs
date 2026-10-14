namespace Acme.Workforce.Assets.Warranty.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive warranty bundles by code.</summary>
public interface IAdaptiveWarrantyBundleStore
{
    AdaptiveWarrantyBundle? Find(string code);
    void Save(AdaptiveWarrantyBundle item);
    bool Remove(string code);
    List<AdaptiveWarrantyBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveWarrantyBundleStore(Dictionary<string, AdaptiveWarrantyBundle> items, List<string> order) : IAdaptiveWarrantyBundleStore
{
    public Dictionary<string, AdaptiveWarrantyBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveWarrantyBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveWarrantyBundle item)
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

    public List<AdaptiveWarrantyBundle> List()
    {
        var outValue = new List<AdaptiveWarrantyBundle>();
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
public sealed class CachedAdaptiveWarrantyBundleStore(MemoryAdaptiveWarrantyBundleStore inner, AdaptiveWarrantyBundleCache cache, int hits, int misses) : IAdaptiveWarrantyBundleStore
{
    public MemoryAdaptiveWarrantyBundleStore Inner { get; set; } = inner;
    public AdaptiveWarrantyBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveWarrantyBundle? Find(string code)
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

    public void Save(AdaptiveWarrantyBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveWarrantyBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive warranty bundles. Empty fields match all.</summary>
public sealed record AdaptiveWarrantyBundleQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveWarrantyBundle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveWarrantyBundle> Apply(List<AdaptiveWarrantyBundle> items)
    {
        var matched = Collections.Filter(items, (AdaptiveWarrantyBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive warranty bundle store.</summary>
public static class AdaptiveWarrantyBundleStore
{
    public static IAdaptiveWarrantyBundleStore NewMemoryAdaptiveWarrantyBundleStore()
    {
        return new MemoryAdaptiveWarrantyBundleStore(
            new Dictionary<string, AdaptiveWarrantyBundle>(),
            new List<string>());
    }

    public static IAdaptiveWarrantyBundleStore NewCachedAdaptiveWarrantyBundleStore(MemoryAdaptiveWarrantyBundleStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveWarrantyBundleStore(
            inner,
            AdaptiveWarrantyBundleCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveWarrantyBundle> FindAdaptiveWarrantyBundles(IAdaptiveWarrantyBundleStore store, AdaptiveWarrantyBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
