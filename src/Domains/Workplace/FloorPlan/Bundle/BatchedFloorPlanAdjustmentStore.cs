namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched floor plan adjustments by code.</summary>
public interface IBatchedFloorPlanAdjustmentStore
{
    BatchedFloorPlanAdjustment? Find(string code);
    void Save(BatchedFloorPlanAdjustment item);
    bool Remove(string code);
    List<BatchedFloorPlanAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedFloorPlanAdjustmentStore(Dictionary<string, BatchedFloorPlanAdjustment> items, List<string> order) : IBatchedFloorPlanAdjustmentStore
{
    public Dictionary<string, BatchedFloorPlanAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedFloorPlanAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedFloorPlanAdjustment item)
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

    public List<BatchedFloorPlanAdjustment> List()
    {
        var outValue = new List<BatchedFloorPlanAdjustment>();
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
public sealed class CachedBatchedFloorPlanAdjustmentStore(MemoryBatchedFloorPlanAdjustmentStore inner, BatchedFloorPlanAdjustmentCache cache, int hits, int misses) : IBatchedFloorPlanAdjustmentStore
{
    public MemoryBatchedFloorPlanAdjustmentStore Inner { get; set; } = inner;
    public BatchedFloorPlanAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedFloorPlanAdjustment? Find(string code)
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

    public void Save(BatchedFloorPlanAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedFloorPlanAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched floor plan adjustments. Empty fields match all.</summary>
public sealed record BatchedFloorPlanAdjustmentQuery(string Team, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedFloorPlanAdjustment item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<BatchedFloorPlanAdjustment> Apply(List<BatchedFloorPlanAdjustment> items)
    {
        var matched = Collections.Filter(items, (BatchedFloorPlanAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched floor plan adjustment store.</summary>
public static class BatchedFloorPlanAdjustmentStore
{
    public static IBatchedFloorPlanAdjustmentStore NewMemoryBatchedFloorPlanAdjustmentStore()
    {
        return new MemoryBatchedFloorPlanAdjustmentStore(
            new Dictionary<string, BatchedFloorPlanAdjustment>(),
            new List<string>());
    }

    public static IBatchedFloorPlanAdjustmentStore NewCachedBatchedFloorPlanAdjustmentStore(MemoryBatchedFloorPlanAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedFloorPlanAdjustmentStore(
            inner,
            BatchedFloorPlanAdjustmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<BatchedFloorPlanAdjustment> FindBatchedFloorPlanAdjustments(IBatchedFloorPlanAdjustmentStore store, BatchedFloorPlanAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
