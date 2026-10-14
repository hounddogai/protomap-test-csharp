namespace Acme.Workforce.Assets.AssetReturn.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted asset return quotas by code.</summary>
public interface IWeightedAssetReturnQuotaStore
{
    WeightedAssetReturnQuota? Find(string code);
    void Save(WeightedAssetReturnQuota item);
    bool Remove(string code);
    List<WeightedAssetReturnQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAssetReturnQuotaStore(Dictionary<string, WeightedAssetReturnQuota> items, List<string> order) : IWeightedAssetReturnQuotaStore
{
    public Dictionary<string, WeightedAssetReturnQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAssetReturnQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAssetReturnQuota item)
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

    public List<WeightedAssetReturnQuota> List()
    {
        var outValue = new List<WeightedAssetReturnQuota>();
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
public sealed class CachedWeightedAssetReturnQuotaStore(MemoryWeightedAssetReturnQuotaStore inner, WeightedAssetReturnQuotaCache cache, int hits, int misses) : IWeightedAssetReturnQuotaStore
{
    public MemoryWeightedAssetReturnQuotaStore Inner { get; set; } = inner;
    public WeightedAssetReturnQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAssetReturnQuota? Find(string code)
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

    public void Save(WeightedAssetReturnQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAssetReturnQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted asset return quotas. Empty fields match all.</summary>
public sealed record WeightedAssetReturnQuotaQuery(string Unit, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAssetReturnQuota item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedAssetReturnQuota> Apply(List<WeightedAssetReturnQuota> items)
    {
        var matched = Collections.Filter(items, (WeightedAssetReturnQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted asset return quota store.</summary>
public static class WeightedAssetReturnQuotaStore
{
    public static IWeightedAssetReturnQuotaStore NewMemoryWeightedAssetReturnQuotaStore()
    {
        return new MemoryWeightedAssetReturnQuotaStore(
            new Dictionary<string, WeightedAssetReturnQuota>(),
            new List<string>());
    }

    public static IWeightedAssetReturnQuotaStore NewCachedWeightedAssetReturnQuotaStore(MemoryWeightedAssetReturnQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAssetReturnQuotaStore(
            inner,
            WeightedAssetReturnQuotaCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<WeightedAssetReturnQuota> FindWeightedAssetReturnQuotas(IWeightedAssetReturnQuotaStore store, WeightedAssetReturnQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
