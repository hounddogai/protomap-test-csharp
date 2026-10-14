namespace Acme.Workforce.Payrollops.PayRun.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run schedules by code.</summary>
public interface IPayRunScheduleStore
{
    PayRunSchedule? Find(string code);
    void Save(PayRunSchedule item);
    bool Remove(string code);
    List<PayRunSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunScheduleStore(Dictionary<string, PayRunSchedule> items, List<string> order) : IPayRunScheduleStore
{
    public Dictionary<string, PayRunSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunSchedule item)
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

    public List<PayRunSchedule> List()
    {
        var outValue = new List<PayRunSchedule>();
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
public sealed class CachedPayRunScheduleStore(MemoryPayRunScheduleStore inner, PayRunScheduleCache cache, int hits, int misses) : IPayRunScheduleStore
{
    public MemoryPayRunScheduleStore Inner { get; set; } = inner;
    public PayRunScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunSchedule? Find(string code)
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

    public void Save(PayRunSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run schedules. Empty fields match all.</summary>
public sealed record PayRunScheduleQuery(string Locale, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunSchedule item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunSchedule> Apply(List<PayRunSchedule> items)
    {
        var matched = Collections.Filter(items, (PayRunSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run schedule store.</summary>
public static class PayRunScheduleStore
{
    public static IPayRunScheduleStore NewMemoryPayRunScheduleStore()
    {
        return new MemoryPayRunScheduleStore(new Dictionary<string, PayRunSchedule>(), new List<string>());
    }

    public static IPayRunScheduleStore NewCachedPayRunScheduleStore(MemoryPayRunScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunScheduleStore(inner, PayRunScheduleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayRunSchedule> FindPayRunSchedules(IPayRunScheduleStore store, PayRunScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
