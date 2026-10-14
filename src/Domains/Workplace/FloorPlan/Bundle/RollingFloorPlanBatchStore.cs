namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling floor plan batches by code.</summary>
public interface IRollingFloorPlanBatchStore
{
    RollingFloorPlanBatch? Find(string code);
    void Save(RollingFloorPlanBatch item);
    bool Remove(string code);
    List<RollingFloorPlanBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingFloorPlanBatchStore(Dictionary<string, RollingFloorPlanBatch> items, List<string> order) : IRollingFloorPlanBatchStore
{
    public Dictionary<string, RollingFloorPlanBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingFloorPlanBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingFloorPlanBatch item)
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

    public List<RollingFloorPlanBatch> List()
    {
        var outValue = new List<RollingFloorPlanBatch>();
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
public sealed class CachedRollingFloorPlanBatchStore(MemoryRollingFloorPlanBatchStore inner, RollingFloorPlanBatchCache cache, int hits, int misses) : IRollingFloorPlanBatchStore
{
    public MemoryRollingFloorPlanBatchStore Inner { get; set; } = inner;
    public RollingFloorPlanBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingFloorPlanBatch? Find(string code)
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

    public void Save(RollingFloorPlanBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingFloorPlanBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling floor plan batches. Empty fields match all.</summary>
public sealed record RollingFloorPlanBatchQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(RollingFloorPlanBatch item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingFloorPlanBatch> Apply(List<RollingFloorPlanBatch> items)
    {
        var matched = Collections.Filter(items, (RollingFloorPlanBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling floor plan batch store.</summary>
public static class RollingFloorPlanBatchStore
{
    public static IRollingFloorPlanBatchStore NewMemoryRollingFloorPlanBatchStore()
    {
        return new MemoryRollingFloorPlanBatchStore(
            new Dictionary<string, RollingFloorPlanBatch>(),
            new List<string>());
    }

    public static IRollingFloorPlanBatchStore NewCachedRollingFloorPlanBatchStore(MemoryRollingFloorPlanBatchStore inner, int ttl, IClock clock)
    {
        return new CachedRollingFloorPlanBatchStore(
            inner,
            RollingFloorPlanBatchCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RollingFloorPlanBatch> FindRollingFloorPlanBatches(IRollingFloorPlanBatchStore store, RollingFloorPlanBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
