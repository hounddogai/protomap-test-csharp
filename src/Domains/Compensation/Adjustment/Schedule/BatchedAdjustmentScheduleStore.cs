namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched adjustment schedules by code.</summary>
public interface IBatchedAdjustmentScheduleStore
{
    BatchedAdjustmentSchedule? Find(string code);
    void Save(BatchedAdjustmentSchedule item);
    bool Remove(string code);
    List<BatchedAdjustmentSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedAdjustmentScheduleStore(Dictionary<string, BatchedAdjustmentSchedule> items, List<string> order) : IBatchedAdjustmentScheduleStore
{
    public Dictionary<string, BatchedAdjustmentSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedAdjustmentSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedAdjustmentSchedule item)
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

    public List<BatchedAdjustmentSchedule> List()
    {
        var outValue = new List<BatchedAdjustmentSchedule>();
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
public sealed class CachedBatchedAdjustmentScheduleStore(MemoryBatchedAdjustmentScheduleStore inner, BatchedAdjustmentScheduleCache cache, int hits, int misses) : IBatchedAdjustmentScheduleStore
{
    public MemoryBatchedAdjustmentScheduleStore Inner { get; set; } = inner;
    public BatchedAdjustmentScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedAdjustmentSchedule? Find(string code)
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

    public void Save(BatchedAdjustmentSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedAdjustmentSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched adjustment schedules. Empty fields match all.</summary>
public sealed record BatchedAdjustmentScheduleQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedAdjustmentSchedule item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedAdjustmentSchedule> Apply(List<BatchedAdjustmentSchedule> items)
    {
        var matched = Collections.Filter(items, (BatchedAdjustmentSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched adjustment schedule store.</summary>
public static class BatchedAdjustmentScheduleStore
{
    public static IBatchedAdjustmentScheduleStore NewMemoryBatchedAdjustmentScheduleStore()
    {
        return new MemoryBatchedAdjustmentScheduleStore(
            new Dictionary<string, BatchedAdjustmentSchedule>(),
            new List<string>());
    }

    public static IBatchedAdjustmentScheduleStore NewCachedBatchedAdjustmentScheduleStore(MemoryBatchedAdjustmentScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedAdjustmentScheduleStore(
            inner,
            BatchedAdjustmentScheduleCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<BatchedAdjustmentSchedule> FindBatchedAdjustmentSchedules(IBatchedAdjustmentScheduleStore store, BatchedAdjustmentScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
