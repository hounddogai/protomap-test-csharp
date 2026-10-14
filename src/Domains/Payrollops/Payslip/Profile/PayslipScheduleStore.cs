namespace Acme.Workforce.Payrollops.Payslip.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip schedules by code.</summary>
public interface IPayslipScheduleStore
{
    PayslipSchedule? Find(string code);
    void Save(PayslipSchedule item);
    bool Remove(string code);
    List<PayslipSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipScheduleStore(Dictionary<string, PayslipSchedule> items, List<string> order) : IPayslipScheduleStore
{
    public Dictionary<string, PayslipSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipSchedule item)
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

    public List<PayslipSchedule> List()
    {
        var outValue = new List<PayslipSchedule>();
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
public sealed class CachedPayslipScheduleStore(MemoryPayslipScheduleStore inner, PayslipScheduleCache cache, int hits, int misses) : IPayslipScheduleStore
{
    public MemoryPayslipScheduleStore Inner { get; set; } = inner;
    public PayslipScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipSchedule? Find(string code)
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

    public void Save(PayslipSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip schedules. Empty fields match all.</summary>
public sealed record PayslipScheduleQuery(string Label, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipSchedule item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<PayslipSchedule> Apply(List<PayslipSchedule> items)
    {
        var matched = Collections.Filter(items, (PayslipSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip schedule store.</summary>
public static class PayslipScheduleStore
{
    public static IPayslipScheduleStore NewMemoryPayslipScheduleStore()
    {
        return new MemoryPayslipScheduleStore(new Dictionary<string, PayslipSchedule>(), new List<string>());
    }

    public static IPayslipScheduleStore NewCachedPayslipScheduleStore(MemoryPayslipScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipScheduleStore(inner, PayslipScheduleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayslipSchedule> FindPayslipSchedules(IPayslipScheduleStore store, PayslipScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
