namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment batches by code.</summary>
public interface IAdjustmentBatchStore
{
    AdjustmentBatch? Find(string code);
    void Save(AdjustmentBatch item);
    bool Remove(string code);
    List<AdjustmentBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentBatchStore(Dictionary<string, AdjustmentBatch> items, List<string> order) : IAdjustmentBatchStore
{
    public Dictionary<string, AdjustmentBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentBatch item)
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

    public List<AdjustmentBatch> List()
    {
        var outValue = new List<AdjustmentBatch>();
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
public sealed class CachedAdjustmentBatchStore(MemoryAdjustmentBatchStore inner, AdjustmentBatchCache cache, int hits, int misses) : IAdjustmentBatchStore
{
    public MemoryAdjustmentBatchStore Inner { get; set; } = inner;
    public AdjustmentBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentBatch? Find(string code)
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

    public void Save(AdjustmentBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment batches. Empty fields match all.</summary>
public sealed record AdjustmentBatchQuery(string Segment, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentBatch item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentBatch> Apply(List<AdjustmentBatch> items)
    {
        var matched = Collections.Filter(items, (AdjustmentBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment batch store.</summary>
public static class AdjustmentBatchStore
{
    public static IAdjustmentBatchStore NewMemoryAdjustmentBatchStore()
    {
        return new MemoryAdjustmentBatchStore(new Dictionary<string, AdjustmentBatch>(), new List<string>());
    }

    public static IAdjustmentBatchStore NewCachedAdjustmentBatchStore(MemoryAdjustmentBatchStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentBatchStore(inner, AdjustmentBatchCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AdjustmentBatch> FindAdjustmentBatches(IAdjustmentBatchStore store, AdjustmentBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
