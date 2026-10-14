namespace Acme.Workforce.Contractors.Timesheet.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet batches by code.</summary>
public interface ITimesheetBatchStore
{
    TimesheetBatch? Find(string code);
    void Save(TimesheetBatch item);
    bool Remove(string code);
    List<TimesheetBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetBatchStore(Dictionary<string, TimesheetBatch> items, List<string> order) : ITimesheetBatchStore
{
    public Dictionary<string, TimesheetBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetBatch item)
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

    public List<TimesheetBatch> List()
    {
        var outValue = new List<TimesheetBatch>();
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
public sealed class CachedTimesheetBatchStore(MemoryTimesheetBatchStore inner, TimesheetBatchCache cache, int hits, int misses) : ITimesheetBatchStore
{
    public MemoryTimesheetBatchStore Inner { get; set; } = inner;
    public TimesheetBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetBatch? Find(string code)
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

    public void Save(TimesheetBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet batches. Empty fields match all.</summary>
public sealed record TimesheetBatchQuery(string Name, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetBatch item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetBatch> Apply(List<TimesheetBatch> items)
    {
        var matched = Collections.Filter(items, (TimesheetBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet batch store.</summary>
public static class TimesheetBatchStore
{
    public static ITimesheetBatchStore NewMemoryTimesheetBatchStore()
    {
        return new MemoryTimesheetBatchStore(new Dictionary<string, TimesheetBatch>(), new List<string>());
    }

    public static ITimesheetBatchStore NewCachedTimesheetBatchStore(MemoryTimesheetBatchStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetBatchStore(inner, TimesheetBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TimesheetBatch> FindTimesheetBatches(ITimesheetBatchStore store, TimesheetBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
