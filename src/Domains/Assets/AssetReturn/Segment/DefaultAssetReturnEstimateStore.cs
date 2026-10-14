namespace Acme.Workforce.Assets.AssetReturn.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default asset return estimates by code.</summary>
public interface IDefaultAssetReturnEstimateStore
{
    DefaultAssetReturnEstimate? Find(string code);
    void Save(DefaultAssetReturnEstimate item);
    bool Remove(string code);
    List<DefaultAssetReturnEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultAssetReturnEstimateStore(Dictionary<string, DefaultAssetReturnEstimate> items, List<string> order) : IDefaultAssetReturnEstimateStore
{
    public Dictionary<string, DefaultAssetReturnEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultAssetReturnEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultAssetReturnEstimate item)
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

    public List<DefaultAssetReturnEstimate> List()
    {
        var outValue = new List<DefaultAssetReturnEstimate>();
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
public sealed class CachedDefaultAssetReturnEstimateStore(MemoryDefaultAssetReturnEstimateStore inner, DefaultAssetReturnEstimateCache cache, int hits, int misses) : IDefaultAssetReturnEstimateStore
{
    public MemoryDefaultAssetReturnEstimateStore Inner { get; set; } = inner;
    public DefaultAssetReturnEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultAssetReturnEstimate? Find(string code)
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

    public void Save(DefaultAssetReturnEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultAssetReturnEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default asset return estimates. Empty fields match all.</summary>
public sealed record DefaultAssetReturnEstimateQuery(string Title, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultAssetReturnEstimate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<DefaultAssetReturnEstimate> Apply(List<DefaultAssetReturnEstimate> items)
    {
        var matched = Collections.Filter(items, (DefaultAssetReturnEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default asset return estimate store.</summary>
public static class DefaultAssetReturnEstimateStore
{
    public static IDefaultAssetReturnEstimateStore NewMemoryDefaultAssetReturnEstimateStore()
    {
        return new MemoryDefaultAssetReturnEstimateStore(
            new Dictionary<string, DefaultAssetReturnEstimate>(),
            new List<string>());
    }

    public static IDefaultAssetReturnEstimateStore NewCachedDefaultAssetReturnEstimateStore(MemoryDefaultAssetReturnEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultAssetReturnEstimateStore(
            inner,
            DefaultAssetReturnEstimateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<DefaultAssetReturnEstimate> FindDefaultAssetReturnEstimates(IDefaultAssetReturnEstimateStore store, DefaultAssetReturnEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
