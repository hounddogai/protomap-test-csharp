namespace Acme.Workforce.Assets.AssetReturn.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal asset return quotas by code.</summary>
public interface ISeasonalAssetReturnQuotaStore
{
    SeasonalAssetReturnQuota? Find(string code);
    void Save(SeasonalAssetReturnQuota item);
    bool Remove(string code);
    List<SeasonalAssetReturnQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalAssetReturnQuotaStore(Dictionary<string, SeasonalAssetReturnQuota> items, List<string> order) : ISeasonalAssetReturnQuotaStore
{
    public Dictionary<string, SeasonalAssetReturnQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalAssetReturnQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalAssetReturnQuota item)
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

    public List<SeasonalAssetReturnQuota> List()
    {
        var outValue = new List<SeasonalAssetReturnQuota>();
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
public sealed class CachedSeasonalAssetReturnQuotaStore(MemorySeasonalAssetReturnQuotaStore inner, SeasonalAssetReturnQuotaCache cache, int hits, int misses) : ISeasonalAssetReturnQuotaStore
{
    public MemorySeasonalAssetReturnQuotaStore Inner { get; set; } = inner;
    public SeasonalAssetReturnQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalAssetReturnQuota? Find(string code)
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

    public void Save(SeasonalAssetReturnQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalAssetReturnQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal asset return quotas. Empty fields match all.</summary>
public sealed record SeasonalAssetReturnQuotaQuery(string Title, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalAssetReturnQuota item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<SeasonalAssetReturnQuota> Apply(List<SeasonalAssetReturnQuota> items)
    {
        var matched = Collections.Filter(items, (SeasonalAssetReturnQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal asset return quota store.</summary>
public static class SeasonalAssetReturnQuotaStore
{
    public static ISeasonalAssetReturnQuotaStore NewMemorySeasonalAssetReturnQuotaStore()
    {
        return new MemorySeasonalAssetReturnQuotaStore(
            new Dictionary<string, SeasonalAssetReturnQuota>(),
            new List<string>());
    }

    public static ISeasonalAssetReturnQuotaStore NewCachedSeasonalAssetReturnQuotaStore(MemorySeasonalAssetReturnQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalAssetReturnQuotaStore(
            inner,
            SeasonalAssetReturnQuotaCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalAssetReturnQuota> FindSeasonalAssetReturnQuotas(ISeasonalAssetReturnQuotaStore store, SeasonalAssetReturnQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
