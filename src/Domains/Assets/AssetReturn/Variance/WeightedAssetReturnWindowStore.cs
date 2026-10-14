namespace Acme.Workforce.Assets.AssetReturn.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted asset return windows by code.</summary>
public interface IWeightedAssetReturnWindowStore
{
    WeightedAssetReturnWindow? Find(string code);
    void Save(WeightedAssetReturnWindow item);
    bool Remove(string code);
    List<WeightedAssetReturnWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAssetReturnWindowStore(Dictionary<string, WeightedAssetReturnWindow> items, List<string> order) : IWeightedAssetReturnWindowStore
{
    public Dictionary<string, WeightedAssetReturnWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAssetReturnWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAssetReturnWindow item)
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

    public List<WeightedAssetReturnWindow> List()
    {
        var outValue = new List<WeightedAssetReturnWindow>();
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
public sealed class CachedWeightedAssetReturnWindowStore(MemoryWeightedAssetReturnWindowStore inner, WeightedAssetReturnWindowCache cache, int hits, int misses) : IWeightedAssetReturnWindowStore
{
    public MemoryWeightedAssetReturnWindowStore Inner { get; set; } = inner;
    public WeightedAssetReturnWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAssetReturnWindow? Find(string code)
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

    public void Save(WeightedAssetReturnWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAssetReturnWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted asset return windows. Empty fields match all.</summary>
public sealed record WeightedAssetReturnWindowQuery(string Segment, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAssetReturnWindow item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedAssetReturnWindow> Apply(List<WeightedAssetReturnWindow> items)
    {
        var matched = Collections.Filter(items, (WeightedAssetReturnWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted asset return window store.</summary>
public static class WeightedAssetReturnWindowStore
{
    public static IWeightedAssetReturnWindowStore NewMemoryWeightedAssetReturnWindowStore()
    {
        return new MemoryWeightedAssetReturnWindowStore(
            new Dictionary<string, WeightedAssetReturnWindow>(),
            new List<string>());
    }

    public static IWeightedAssetReturnWindowStore NewCachedWeightedAssetReturnWindowStore(MemoryWeightedAssetReturnWindowStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAssetReturnWindowStore(
            inner,
            WeightedAssetReturnWindowCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<WeightedAssetReturnWindow> FindWeightedAssetReturnWindows(IWeightedAssetReturnWindowStore store, WeightedAssetReturnWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
