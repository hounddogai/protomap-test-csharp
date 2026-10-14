namespace Acme.Workforce.Assets.AssetReturn.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback asset return segments by code.</summary>
public interface IFallbackAssetReturnSegmentStore
{
    FallbackAssetReturnSegment? Find(string code);
    void Save(FallbackAssetReturnSegment item);
    bool Remove(string code);
    List<FallbackAssetReturnSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackAssetReturnSegmentStore(Dictionary<string, FallbackAssetReturnSegment> items, List<string> order) : IFallbackAssetReturnSegmentStore
{
    public Dictionary<string, FallbackAssetReturnSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackAssetReturnSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackAssetReturnSegment item)
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

    public List<FallbackAssetReturnSegment> List()
    {
        var outValue = new List<FallbackAssetReturnSegment>();
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
public sealed class CachedFallbackAssetReturnSegmentStore(MemoryFallbackAssetReturnSegmentStore inner, FallbackAssetReturnSegmentCache cache, int hits, int misses) : IFallbackAssetReturnSegmentStore
{
    public MemoryFallbackAssetReturnSegmentStore Inner { get; set; } = inner;
    public FallbackAssetReturnSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackAssetReturnSegment? Find(string code)
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

    public void Save(FallbackAssetReturnSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackAssetReturnSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback asset return segments. Empty fields match all.</summary>
public sealed record FallbackAssetReturnSegmentQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackAssetReturnSegment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackAssetReturnSegment> Apply(List<FallbackAssetReturnSegment> items)
    {
        var matched = Collections.Filter(items, (FallbackAssetReturnSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback asset return segment store.</summary>
public static class FallbackAssetReturnSegmentStore
{
    public static IFallbackAssetReturnSegmentStore NewMemoryFallbackAssetReturnSegmentStore()
    {
        return new MemoryFallbackAssetReturnSegmentStore(
            new Dictionary<string, FallbackAssetReturnSegment>(),
            new List<string>());
    }

    public static IFallbackAssetReturnSegmentStore NewCachedFallbackAssetReturnSegmentStore(MemoryFallbackAssetReturnSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackAssetReturnSegmentStore(
            inner,
            FallbackAssetReturnSegmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<FallbackAssetReturnSegment> FindFallbackAssetReturnSegments(IFallbackAssetReturnSegmentStore store, FallbackAssetReturnSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
