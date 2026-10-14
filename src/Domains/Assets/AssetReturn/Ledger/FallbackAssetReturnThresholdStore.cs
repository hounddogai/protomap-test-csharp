namespace Acme.Workforce.Assets.AssetReturn.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback asset return thresholds by code.</summary>
public interface IFallbackAssetReturnThresholdStore
{
    FallbackAssetReturnThreshold? Find(string code);
    void Save(FallbackAssetReturnThreshold item);
    bool Remove(string code);
    List<FallbackAssetReturnThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackAssetReturnThresholdStore(Dictionary<string, FallbackAssetReturnThreshold> items, List<string> order) : IFallbackAssetReturnThresholdStore
{
    public Dictionary<string, FallbackAssetReturnThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackAssetReturnThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackAssetReturnThreshold item)
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

    public List<FallbackAssetReturnThreshold> List()
    {
        var outValue = new List<FallbackAssetReturnThreshold>();
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
public sealed class CachedFallbackAssetReturnThresholdStore(MemoryFallbackAssetReturnThresholdStore inner, FallbackAssetReturnThresholdCache cache, int hits, int misses) : IFallbackAssetReturnThresholdStore
{
    public MemoryFallbackAssetReturnThresholdStore Inner { get; set; } = inner;
    public FallbackAssetReturnThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackAssetReturnThreshold? Find(string code)
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

    public void Save(FallbackAssetReturnThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackAssetReturnThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback asset return thresholds. Empty fields match all.</summary>
public sealed record FallbackAssetReturnThresholdQuery(string Label, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackAssetReturnThreshold item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackAssetReturnThreshold> Apply(List<FallbackAssetReturnThreshold> items)
    {
        var matched = Collections.Filter(items, (FallbackAssetReturnThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback asset return threshold store.</summary>
public static class FallbackAssetReturnThresholdStore
{
    public static IFallbackAssetReturnThresholdStore NewMemoryFallbackAssetReturnThresholdStore()
    {
        return new MemoryFallbackAssetReturnThresholdStore(
            new Dictionary<string, FallbackAssetReturnThreshold>(),
            new List<string>());
    }

    public static IFallbackAssetReturnThresholdStore NewCachedFallbackAssetReturnThresholdStore(MemoryFallbackAssetReturnThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackAssetReturnThresholdStore(
            inner,
            FallbackAssetReturnThresholdCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackAssetReturnThreshold> FindFallbackAssetReturnThresholds(IFallbackAssetReturnThresholdStore store, FallbackAssetReturnThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
