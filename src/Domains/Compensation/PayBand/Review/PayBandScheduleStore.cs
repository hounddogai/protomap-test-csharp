namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band schedules by code.</summary>
public interface IPayBandScheduleStore
{
    PayBandSchedule? Find(string code);
    void Save(PayBandSchedule item);
    bool Remove(string code);
    List<PayBandSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandScheduleStore(Dictionary<string, PayBandSchedule> items, List<string> order) : IPayBandScheduleStore
{
    public Dictionary<string, PayBandSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandSchedule item)
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

    public List<PayBandSchedule> List()
    {
        var outValue = new List<PayBandSchedule>();
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
public sealed class CachedPayBandScheduleStore(MemoryPayBandScheduleStore inner, PayBandScheduleCache cache, int hits, int misses) : IPayBandScheduleStore
{
    public MemoryPayBandScheduleStore Inner { get; set; } = inner;
    public PayBandScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandSchedule? Find(string code)
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

    public void Save(PayBandSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band schedules. Empty fields match all.</summary>
public sealed record PayBandScheduleQuery(string Department, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandSchedule item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandSchedule> Apply(List<PayBandSchedule> items)
    {
        var matched = Collections.Filter(items, (PayBandSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band schedule store.</summary>
public static class PayBandScheduleStore
{
    public static IPayBandScheduleStore NewMemoryPayBandScheduleStore()
    {
        return new MemoryPayBandScheduleStore(new Dictionary<string, PayBandSchedule>(), new List<string>());
    }

    public static IPayBandScheduleStore NewCachedPayBandScheduleStore(MemoryPayBandScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandScheduleStore(inner, PayBandScheduleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayBandSchedule> FindPayBandSchedules(IPayBandScheduleStore store, PayBandScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
