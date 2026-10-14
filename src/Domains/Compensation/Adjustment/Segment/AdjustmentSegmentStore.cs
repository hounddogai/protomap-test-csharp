namespace Acme.Workforce.Compensation.Adjustment.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment segments by code.</summary>
public interface IAdjustmentSegmentStore
{
    AdjustmentSegment? Find(string code);
    void Save(AdjustmentSegment item);
    bool Remove(string code);
    List<AdjustmentSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentSegmentStore(Dictionary<string, AdjustmentSegment> items, List<string> order) : IAdjustmentSegmentStore
{
    public Dictionary<string, AdjustmentSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentSegment item)
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

    public List<AdjustmentSegment> List()
    {
        var outValue = new List<AdjustmentSegment>();
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
public sealed class CachedAdjustmentSegmentStore(MemoryAdjustmentSegmentStore inner, AdjustmentSegmentCache cache, int hits, int misses) : IAdjustmentSegmentStore
{
    public MemoryAdjustmentSegmentStore Inner { get; set; } = inner;
    public AdjustmentSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentSegment? Find(string code)
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

    public void Save(AdjustmentSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment segments. Empty fields match all.</summary>
public sealed record AdjustmentSegmentQuery(string Category, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentSegment item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentSegment> Apply(List<AdjustmentSegment> items)
    {
        var matched = Collections.Filter(items, (AdjustmentSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment segment store.</summary>
public static class AdjustmentSegmentStore
{
    public static IAdjustmentSegmentStore NewMemoryAdjustmentSegmentStore()
    {
        return new MemoryAdjustmentSegmentStore(new Dictionary<string, AdjustmentSegment>(), new List<string>());
    }

    public static IAdjustmentSegmentStore NewCachedAdjustmentSegmentStore(MemoryAdjustmentSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentSegmentStore(inner, AdjustmentSegmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AdjustmentSegment> FindAdjustmentSegments(IAdjustmentSegmentStore store, AdjustmentSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
