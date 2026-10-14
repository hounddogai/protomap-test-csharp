namespace Acme.Workforce.Engagement.ActionPlan.PlanSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback action plan segments by code.</summary>
public interface IFallbackActionPlanSegmentStore
{
    FallbackActionPlanSegment? Find(string code);
    void Save(FallbackActionPlanSegment item);
    bool Remove(string code);
    List<FallbackActionPlanSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackActionPlanSegmentStore(Dictionary<string, FallbackActionPlanSegment> items, List<string> order) : IFallbackActionPlanSegmentStore
{
    public Dictionary<string, FallbackActionPlanSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackActionPlanSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackActionPlanSegment item)
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

    public List<FallbackActionPlanSegment> List()
    {
        var outValue = new List<FallbackActionPlanSegment>();
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
public sealed class CachedFallbackActionPlanSegmentStore(MemoryFallbackActionPlanSegmentStore inner, FallbackActionPlanSegmentCache cache, int hits, int misses) : IFallbackActionPlanSegmentStore
{
    public MemoryFallbackActionPlanSegmentStore Inner { get; set; } = inner;
    public FallbackActionPlanSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackActionPlanSegment? Find(string code)
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

    public void Save(FallbackActionPlanSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackActionPlanSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback action plan segments. Empty fields match all.</summary>
public sealed record FallbackActionPlanSegmentQuery(string Segment, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackActionPlanSegment item)
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

    public List<FallbackActionPlanSegment> Apply(List<FallbackActionPlanSegment> items)
    {
        var matched = Collections.Filter(items, (FallbackActionPlanSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback action plan segment store.</summary>
public static class FallbackActionPlanSegmentStore
{
    public static IFallbackActionPlanSegmentStore NewMemoryFallbackActionPlanSegmentStore()
    {
        return new MemoryFallbackActionPlanSegmentStore(
            new Dictionary<string, FallbackActionPlanSegment>(),
            new List<string>());
    }

    public static IFallbackActionPlanSegmentStore NewCachedFallbackActionPlanSegmentStore(MemoryFallbackActionPlanSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackActionPlanSegmentStore(
            inner,
            FallbackActionPlanSegmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<FallbackActionPlanSegment> FindFallbackActionPlanSegments(IFallbackActionPlanSegmentStore store, FallbackActionPlanSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
