namespace Acme.Workforce.Assets.AssetReturn.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive asset return variances by code.</summary>
public interface IAdaptiveAssetReturnVarianceStore
{
    AdaptiveAssetReturnVariance? Find(string code);
    void Save(AdaptiveAssetReturnVariance item);
    bool Remove(string code);
    List<AdaptiveAssetReturnVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveAssetReturnVarianceStore(Dictionary<string, AdaptiveAssetReturnVariance> items, List<string> order) : IAdaptiveAssetReturnVarianceStore
{
    public Dictionary<string, AdaptiveAssetReturnVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveAssetReturnVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveAssetReturnVariance item)
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

    public List<AdaptiveAssetReturnVariance> List()
    {
        var outValue = new List<AdaptiveAssetReturnVariance>();
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
public sealed class CachedAdaptiveAssetReturnVarianceStore(MemoryAdaptiveAssetReturnVarianceStore inner, AdaptiveAssetReturnVarianceCache cache, int hits, int misses) : IAdaptiveAssetReturnVarianceStore
{
    public MemoryAdaptiveAssetReturnVarianceStore Inner { get; set; } = inner;
    public AdaptiveAssetReturnVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveAssetReturnVariance? Find(string code)
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

    public void Save(AdaptiveAssetReturnVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveAssetReturnVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive asset return variances. Empty fields match all.</summary>
public sealed record AdaptiveAssetReturnVarianceQuery(string Name, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveAssetReturnVariance item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveAssetReturnVariance> Apply(List<AdaptiveAssetReturnVariance> items)
    {
        var matched = Collections.Filter(items, (AdaptiveAssetReturnVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive asset return variance store.</summary>
public static class AdaptiveAssetReturnVarianceStore
{
    public static IAdaptiveAssetReturnVarianceStore NewMemoryAdaptiveAssetReturnVarianceStore()
    {
        return new MemoryAdaptiveAssetReturnVarianceStore(
            new Dictionary<string, AdaptiveAssetReturnVariance>(),
            new List<string>());
    }

    public static IAdaptiveAssetReturnVarianceStore NewCachedAdaptiveAssetReturnVarianceStore(MemoryAdaptiveAssetReturnVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveAssetReturnVarianceStore(
            inner,
            AdaptiveAssetReturnVarianceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveAssetReturnVariance> FindAdaptiveAssetReturnVariances(IAdaptiveAssetReturnVarianceStore store, AdaptiveAssetReturnVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
