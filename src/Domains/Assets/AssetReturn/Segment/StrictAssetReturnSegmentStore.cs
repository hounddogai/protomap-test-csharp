namespace Acme.Workforce.Assets.AssetReturn.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict asset return segments by code.</summary>
public interface IStrictAssetReturnSegmentStore
{
    StrictAssetReturnSegment? Find(string code);
    void Save(StrictAssetReturnSegment item);
    bool Remove(string code);
    List<StrictAssetReturnSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictAssetReturnSegmentStore(Dictionary<string, StrictAssetReturnSegment> items, List<string> order) : IStrictAssetReturnSegmentStore
{
    public Dictionary<string, StrictAssetReturnSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictAssetReturnSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictAssetReturnSegment item)
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

    public List<StrictAssetReturnSegment> List()
    {
        var outValue = new List<StrictAssetReturnSegment>();
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
public sealed class CachedStrictAssetReturnSegmentStore(MemoryStrictAssetReturnSegmentStore inner, StrictAssetReturnSegmentCache cache, int hits, int misses) : IStrictAssetReturnSegmentStore
{
    public MemoryStrictAssetReturnSegmentStore Inner { get; set; } = inner;
    public StrictAssetReturnSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictAssetReturnSegment? Find(string code)
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

    public void Save(StrictAssetReturnSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictAssetReturnSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict asset return segments. Empty fields match all.</summary>
public sealed record StrictAssetReturnSegmentQuery(string Source, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(StrictAssetReturnSegment item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictAssetReturnSegment> Apply(List<StrictAssetReturnSegment> items)
    {
        var matched = Collections.Filter(items, (StrictAssetReturnSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict asset return segment store.</summary>
public static class StrictAssetReturnSegmentStore
{
    public static IStrictAssetReturnSegmentStore NewMemoryStrictAssetReturnSegmentStore()
    {
        return new MemoryStrictAssetReturnSegmentStore(
            new Dictionary<string, StrictAssetReturnSegment>(),
            new List<string>());
    }

    public static IStrictAssetReturnSegmentStore NewCachedStrictAssetReturnSegmentStore(MemoryStrictAssetReturnSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedStrictAssetReturnSegmentStore(
            inner,
            StrictAssetReturnSegmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<StrictAssetReturnSegment> FindStrictAssetReturnSegments(IStrictAssetReturnSegmentStore store, StrictAssetReturnSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
