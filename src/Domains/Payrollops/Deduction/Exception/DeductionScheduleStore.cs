namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction schedules by code.</summary>
public interface IDeductionScheduleStore
{
    DeductionSchedule? Find(string code);
    void Save(DeductionSchedule item);
    bool Remove(string code);
    List<DeductionSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionScheduleStore(Dictionary<string, DeductionSchedule> items, List<string> order) : IDeductionScheduleStore
{
    public Dictionary<string, DeductionSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionSchedule item)
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

    public List<DeductionSchedule> List()
    {
        var outValue = new List<DeductionSchedule>();
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
public sealed class CachedDeductionScheduleStore(MemoryDeductionScheduleStore inner, DeductionScheduleCache cache, int hits, int misses) : IDeductionScheduleStore
{
    public MemoryDeductionScheduleStore Inner { get; set; } = inner;
    public DeductionScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionSchedule? Find(string code)
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

    public void Save(DeductionSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction schedules. Empty fields match all.</summary>
public sealed record DeductionScheduleQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionSchedule item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionSchedule> Apply(List<DeductionSchedule> items)
    {
        var matched = Collections.Filter(items, (DeductionSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction schedule store.</summary>
public static class DeductionScheduleStore
{
    public static IDeductionScheduleStore NewMemoryDeductionScheduleStore()
    {
        return new MemoryDeductionScheduleStore(new Dictionary<string, DeductionSchedule>(), new List<string>());
    }

    public static IDeductionScheduleStore NewCachedDeductionScheduleStore(MemoryDeductionScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionScheduleStore(inner, DeductionScheduleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DeductionSchedule> FindDeductionSchedules(IDeductionScheduleStore store, DeductionScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
