namespace Acme.Workforce.Payrollops.Deduction.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional deduction schedules by code.</summary>
public interface IRegionalDeductionScheduleStore
{
    RegionalDeductionSchedule? Find(string code);
    void Save(RegionalDeductionSchedule item);
    bool Remove(string code);
    List<RegionalDeductionSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalDeductionScheduleStore(Dictionary<string, RegionalDeductionSchedule> items, List<string> order) : IRegionalDeductionScheduleStore
{
    public Dictionary<string, RegionalDeductionSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalDeductionSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalDeductionSchedule item)
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

    public List<RegionalDeductionSchedule> List()
    {
        var outValue = new List<RegionalDeductionSchedule>();
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
public sealed class CachedRegionalDeductionScheduleStore(MemoryRegionalDeductionScheduleStore inner, RegionalDeductionScheduleCache cache, int hits, int misses) : IRegionalDeductionScheduleStore
{
    public MemoryRegionalDeductionScheduleStore Inner { get; set; } = inner;
    public RegionalDeductionScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalDeductionSchedule? Find(string code)
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

    public void Save(RegionalDeductionSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalDeductionSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional deduction schedules. Empty fields match all.</summary>
public sealed record RegionalDeductionScheduleQuery(string Department, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalDeductionSchedule item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<RegionalDeductionSchedule> Apply(List<RegionalDeductionSchedule> items)
    {
        var matched = Collections.Filter(items, (RegionalDeductionSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional deduction schedule store.</summary>
public static class RegionalDeductionScheduleStore
{
    public static IRegionalDeductionScheduleStore NewMemoryRegionalDeductionScheduleStore()
    {
        return new MemoryRegionalDeductionScheduleStore(
            new Dictionary<string, RegionalDeductionSchedule>(),
            new List<string>());
    }

    public static IRegionalDeductionScheduleStore NewCachedRegionalDeductionScheduleStore(MemoryRegionalDeductionScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalDeductionScheduleStore(
            inner,
            RegionalDeductionScheduleCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RegionalDeductionSchedule> FindRegionalDeductionSchedules(IRegionalDeductionScheduleStore store, RegionalDeductionScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
