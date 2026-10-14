namespace Acme.Workforce.Absence.Policy.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive policy bundles by code.</summary>
public interface IAdaptivePolicyBundleStore
{
    AdaptivePolicyBundle? Find(string code);
    void Save(AdaptivePolicyBundle item);
    bool Remove(string code);
    List<AdaptivePolicyBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptivePolicyBundleStore(Dictionary<string, AdaptivePolicyBundle> items, List<string> order) : IAdaptivePolicyBundleStore
{
    public Dictionary<string, AdaptivePolicyBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptivePolicyBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptivePolicyBundle item)
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

    public List<AdaptivePolicyBundle> List()
    {
        var outValue = new List<AdaptivePolicyBundle>();
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
public sealed class CachedAdaptivePolicyBundleStore(MemoryAdaptivePolicyBundleStore inner, AdaptivePolicyBundleCache cache, int hits, int misses) : IAdaptivePolicyBundleStore
{
    public MemoryAdaptivePolicyBundleStore Inner { get; set; } = inner;
    public AdaptivePolicyBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptivePolicyBundle? Find(string code)
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

    public void Save(AdaptivePolicyBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptivePolicyBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive policy bundles. Empty fields match all.</summary>
public sealed record AdaptivePolicyBundleQuery(string Locale, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptivePolicyBundle item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptivePolicyBundle> Apply(List<AdaptivePolicyBundle> items)
    {
        var matched = Collections.Filter(items, (AdaptivePolicyBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive policy bundle store.</summary>
public static class AdaptivePolicyBundleStore
{
    public static IAdaptivePolicyBundleStore NewMemoryAdaptivePolicyBundleStore()
    {
        return new MemoryAdaptivePolicyBundleStore(
            new Dictionary<string, AdaptivePolicyBundle>(),
            new List<string>());
    }

    public static IAdaptivePolicyBundleStore NewCachedAdaptivePolicyBundleStore(MemoryAdaptivePolicyBundleStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptivePolicyBundleStore(inner, AdaptivePolicyBundleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AdaptivePolicyBundle> FindAdaptivePolicyBundles(IAdaptivePolicyBundleStore store, AdaptivePolicyBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
