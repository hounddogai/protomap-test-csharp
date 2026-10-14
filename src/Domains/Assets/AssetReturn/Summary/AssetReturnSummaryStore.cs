namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return summaries by code.</summary>
public interface IAssetReturnSummaryStore
{
    AssetReturnSummary? Find(string code);
    void Save(AssetReturnSummary item);
    bool Remove(string code);
    List<AssetReturnSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnSummaryStore(Dictionary<string, AssetReturnSummary> items, List<string> order) : IAssetReturnSummaryStore
{
    public Dictionary<string, AssetReturnSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnSummary item)
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

    public List<AssetReturnSummary> List()
    {
        var outValue = new List<AssetReturnSummary>();
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
public sealed class CachedAssetReturnSummaryStore(MemoryAssetReturnSummaryStore inner, AssetReturnSummaryCache cache, int hits, int misses) : IAssetReturnSummaryStore
{
    public MemoryAssetReturnSummaryStore Inner { get; set; } = inner;
    public AssetReturnSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnSummary? Find(string code)
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

    public void Save(AssetReturnSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return summaries. Empty fields match all.</summary>
public sealed record AssetReturnSummaryQuery(string Segment, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnSummary item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<AssetReturnSummary> Apply(List<AssetReturnSummary> items)
    {
        var matched = Collections.Filter(items, (AssetReturnSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return summary store.</summary>
public static class AssetReturnSummaryStore
{
    public static IAssetReturnSummaryStore NewMemoryAssetReturnSummaryStore()
    {
        return new MemoryAssetReturnSummaryStore(new Dictionary<string, AssetReturnSummary>(), new List<string>());
    }

    public static IAssetReturnSummaryStore NewCachedAssetReturnSummaryStore(MemoryAssetReturnSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnSummaryStore(inner, AssetReturnSummaryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AssetReturnSummary> FindAssetReturnSummaries(IAssetReturnSummaryStore store, AssetReturnSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
