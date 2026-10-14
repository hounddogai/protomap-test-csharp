namespace Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched holiday schedules by code.</summary>
public interface IBatchedHolidayScheduleStore
{
    BatchedHolidaySchedule? Find(string code);
    void Save(BatchedHolidaySchedule item);
    bool Remove(string code);
    List<BatchedHolidaySchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedHolidayScheduleStore(Dictionary<string, BatchedHolidaySchedule> items, List<string> order) : IBatchedHolidayScheduleStore
{
    public Dictionary<string, BatchedHolidaySchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedHolidaySchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedHolidaySchedule item)
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

    public List<BatchedHolidaySchedule> List()
    {
        var outValue = new List<BatchedHolidaySchedule>();
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
public sealed class CachedBatchedHolidayScheduleStore(MemoryBatchedHolidayScheduleStore inner, BatchedHolidayScheduleCache cache, int hits, int misses) : IBatchedHolidayScheduleStore
{
    public MemoryBatchedHolidayScheduleStore Inner { get; set; } = inner;
    public BatchedHolidayScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedHolidaySchedule? Find(string code)
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

    public void Save(BatchedHolidaySchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedHolidaySchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched holiday schedules. Empty fields match all.</summary>
public sealed record BatchedHolidayScheduleQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedHolidaySchedule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedHolidaySchedule> Apply(List<BatchedHolidaySchedule> items)
    {
        var matched = Collections.Filter(items, (BatchedHolidaySchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched holiday schedule store.</summary>
public static class BatchedHolidayScheduleStore
{
    public static IBatchedHolidayScheduleStore NewMemoryBatchedHolidayScheduleStore()
    {
        return new MemoryBatchedHolidayScheduleStore(
            new Dictionary<string, BatchedHolidaySchedule>(),
            new List<string>());
    }

    public static IBatchedHolidayScheduleStore NewCachedBatchedHolidayScheduleStore(MemoryBatchedHolidayScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedHolidayScheduleStore(
            inner,
            BatchedHolidayScheduleCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<BatchedHolidaySchedule> FindBatchedHolidaySchedules(IBatchedHolidayScheduleStore store, BatchedHolidayScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
