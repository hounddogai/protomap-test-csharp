namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy adjustment batches by code.</summary>
public interface ILegacyAdjustmentBatchStore
{
    LegacyAdjustmentBatch? Find(string code);
    void Save(LegacyAdjustmentBatch item);
    bool Remove(string code);
    List<LegacyAdjustmentBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyAdjustmentBatchStore(Dictionary<string, LegacyAdjustmentBatch> items, List<string> order) : ILegacyAdjustmentBatchStore
{
    public Dictionary<string, LegacyAdjustmentBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyAdjustmentBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyAdjustmentBatch item)
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

    public List<LegacyAdjustmentBatch> List()
    {
        var outValue = new List<LegacyAdjustmentBatch>();
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
public sealed class CachedLegacyAdjustmentBatchStore(MemoryLegacyAdjustmentBatchStore inner, LegacyAdjustmentBatchCache cache, int hits, int misses) : ILegacyAdjustmentBatchStore
{
    public MemoryLegacyAdjustmentBatchStore Inner { get; set; } = inner;
    public LegacyAdjustmentBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyAdjustmentBatch? Find(string code)
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

    public void Save(LegacyAdjustmentBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyAdjustmentBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy adjustment batches. Empty fields match all.</summary>
public sealed record LegacyAdjustmentBatchQuery(string Team, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyAdjustmentBatch item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyAdjustmentBatch> Apply(List<LegacyAdjustmentBatch> items)
    {
        var matched = Collections.Filter(items, (LegacyAdjustmentBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy adjustment batch store.</summary>
public static class LegacyAdjustmentBatchStore
{
    public static ILegacyAdjustmentBatchStore NewMemoryLegacyAdjustmentBatchStore()
    {
        return new MemoryLegacyAdjustmentBatchStore(
            new Dictionary<string, LegacyAdjustmentBatch>(),
            new List<string>());
    }

    public static ILegacyAdjustmentBatchStore NewCachedLegacyAdjustmentBatchStore(MemoryLegacyAdjustmentBatchStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyAdjustmentBatchStore(
            inner,
            LegacyAdjustmentBatchCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LegacyAdjustmentBatch> FindLegacyAdjustmentBatches(ILegacyAdjustmentBatchStore store, LegacyAdjustmentBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
