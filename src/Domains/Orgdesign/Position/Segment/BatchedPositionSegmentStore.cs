namespace Acme.Workforce.Orgdesign.Position.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched position segments by code.</summary>
public interface IBatchedPositionSegmentStore
{
    BatchedPositionSegment? Find(string code);
    void Save(BatchedPositionSegment item);
    bool Remove(string code);
    List<BatchedPositionSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedPositionSegmentStore(Dictionary<string, BatchedPositionSegment> items, List<string> order) : IBatchedPositionSegmentStore
{
    public Dictionary<string, BatchedPositionSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedPositionSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedPositionSegment item)
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

    public List<BatchedPositionSegment> List()
    {
        var outValue = new List<BatchedPositionSegment>();
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
public sealed class CachedBatchedPositionSegmentStore(MemoryBatchedPositionSegmentStore inner, BatchedPositionSegmentCache cache, int hits, int misses) : IBatchedPositionSegmentStore
{
    public MemoryBatchedPositionSegmentStore Inner { get; set; } = inner;
    public BatchedPositionSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedPositionSegment? Find(string code)
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

    public void Save(BatchedPositionSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedPositionSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched position segments. Empty fields match all.</summary>
public sealed record BatchedPositionSegmentQuery(string Category, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedPositionSegment item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<BatchedPositionSegment> Apply(List<BatchedPositionSegment> items)
    {
        var matched = Collections.Filter(items, (BatchedPositionSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched position segment store.</summary>
public static class BatchedPositionSegmentStore
{
    public static IBatchedPositionSegmentStore NewMemoryBatchedPositionSegmentStore()
    {
        return new MemoryBatchedPositionSegmentStore(
            new Dictionary<string, BatchedPositionSegment>(),
            new List<string>());
    }

    public static IBatchedPositionSegmentStore NewCachedBatchedPositionSegmentStore(MemoryBatchedPositionSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedPositionSegmentStore(
            inner,
            BatchedPositionSegmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<BatchedPositionSegment> FindBatchedPositionSegments(IBatchedPositionSegmentStore store, BatchedPositionSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
