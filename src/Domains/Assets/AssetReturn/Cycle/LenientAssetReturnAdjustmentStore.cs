namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient asset return adjustments by code.</summary>
public interface ILenientAssetReturnAdjustmentStore
{
    LenientAssetReturnAdjustment? Find(string code);
    void Save(LenientAssetReturnAdjustment item);
    bool Remove(string code);
    List<LenientAssetReturnAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientAssetReturnAdjustmentStore(Dictionary<string, LenientAssetReturnAdjustment> items, List<string> order) : ILenientAssetReturnAdjustmentStore
{
    public Dictionary<string, LenientAssetReturnAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientAssetReturnAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientAssetReturnAdjustment item)
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

    public List<LenientAssetReturnAdjustment> List()
    {
        var outValue = new List<LenientAssetReturnAdjustment>();
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
public sealed class CachedLenientAssetReturnAdjustmentStore(MemoryLenientAssetReturnAdjustmentStore inner, LenientAssetReturnAdjustmentCache cache, int hits, int misses) : ILenientAssetReturnAdjustmentStore
{
    public MemoryLenientAssetReturnAdjustmentStore Inner { get; set; } = inner;
    public LenientAssetReturnAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientAssetReturnAdjustment? Find(string code)
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

    public void Save(LenientAssetReturnAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientAssetReturnAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient asset return adjustments. Empty fields match all.</summary>
public sealed record LenientAssetReturnAdjustmentQuery(string Title, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(LenientAssetReturnAdjustment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientAssetReturnAdjustment> Apply(List<LenientAssetReturnAdjustment> items)
    {
        var matched = Collections.Filter(items, (LenientAssetReturnAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient asset return adjustment store.</summary>
public static class LenientAssetReturnAdjustmentStore
{
    public static ILenientAssetReturnAdjustmentStore NewMemoryLenientAssetReturnAdjustmentStore()
    {
        return new MemoryLenientAssetReturnAdjustmentStore(
            new Dictionary<string, LenientAssetReturnAdjustment>(),
            new List<string>());
    }

    public static ILenientAssetReturnAdjustmentStore NewCachedLenientAssetReturnAdjustmentStore(MemoryLenientAssetReturnAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedLenientAssetReturnAdjustmentStore(
            inner,
            LenientAssetReturnAdjustmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LenientAssetReturnAdjustment> FindLenientAssetReturnAdjustments(ILenientAssetReturnAdjustmentStore store, LenientAssetReturnAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
