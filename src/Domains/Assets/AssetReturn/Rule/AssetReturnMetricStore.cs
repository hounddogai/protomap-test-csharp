namespace Acme.Workforce.Assets.AssetReturn.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return metrics by code.</summary>
public interface IAssetReturnMetricStore
{
    AssetReturnMetric? Find(string code);
    void Save(AssetReturnMetric item);
    bool Remove(string code);
    List<AssetReturnMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnMetricStore(Dictionary<string, AssetReturnMetric> items, List<string> order) : IAssetReturnMetricStore
{
    public Dictionary<string, AssetReturnMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnMetric item)
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

    public List<AssetReturnMetric> List()
    {
        var outValue = new List<AssetReturnMetric>();
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
public sealed class CachedAssetReturnMetricStore(MemoryAssetReturnMetricStore inner, AssetReturnMetricCache cache, int hits, int misses) : IAssetReturnMetricStore
{
    public MemoryAssetReturnMetricStore Inner { get; set; } = inner;
    public AssetReturnMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnMetric? Find(string code)
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

    public void Save(AssetReturnMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return metrics. Empty fields match all.</summary>
public sealed record AssetReturnMetricQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnMetric item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<AssetReturnMetric> Apply(List<AssetReturnMetric> items)
    {
        var matched = Collections.Filter(items, (AssetReturnMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return metric store.</summary>
public static class AssetReturnMetricStore
{
    public static IAssetReturnMetricStore NewMemoryAssetReturnMetricStore()
    {
        return new MemoryAssetReturnMetricStore(new Dictionary<string, AssetReturnMetric>(), new List<string>());
    }

    public static IAssetReturnMetricStore NewCachedAssetReturnMetricStore(MemoryAssetReturnMetricStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnMetricStore(inner, AssetReturnMetricCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AssetReturnMetric> FindAssetReturnMetrics(IAssetReturnMetricStore store, AssetReturnMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
