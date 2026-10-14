namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary accrual schedules by code.</summary>
public interface IPrimaryAccrualScheduleStore
{
    PrimaryAccrualSchedule? Find(string code);
    void Save(PrimaryAccrualSchedule item);
    bool Remove(string code);
    List<PrimaryAccrualSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAccrualScheduleStore(Dictionary<string, PrimaryAccrualSchedule> items, List<string> order) : IPrimaryAccrualScheduleStore
{
    public Dictionary<string, PrimaryAccrualSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAccrualSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAccrualSchedule item)
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

    public List<PrimaryAccrualSchedule> List()
    {
        var outValue = new List<PrimaryAccrualSchedule>();
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
public sealed class CachedPrimaryAccrualScheduleStore(MemoryPrimaryAccrualScheduleStore inner, PrimaryAccrualScheduleCache cache, int hits, int misses) : IPrimaryAccrualScheduleStore
{
    public MemoryPrimaryAccrualScheduleStore Inner { get; set; } = inner;
    public PrimaryAccrualScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAccrualSchedule? Find(string code)
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

    public void Save(PrimaryAccrualSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAccrualSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary accrual schedules. Empty fields match all.</summary>
public sealed record PrimaryAccrualScheduleQuery(string Channel, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAccrualSchedule item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryAccrualSchedule> Apply(List<PrimaryAccrualSchedule> items)
    {
        var matched = Collections.Filter(items, (PrimaryAccrualSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary accrual schedule store.</summary>
public static class PrimaryAccrualScheduleStore
{
    public static IPrimaryAccrualScheduleStore NewMemoryPrimaryAccrualScheduleStore()
    {
        return new MemoryPrimaryAccrualScheduleStore(
            new Dictionary<string, PrimaryAccrualSchedule>(),
            new List<string>());
    }

    public static IPrimaryAccrualScheduleStore NewCachedPrimaryAccrualScheduleStore(MemoryPrimaryAccrualScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAccrualScheduleStore(
            inner,
            PrimaryAccrualScheduleCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryAccrualSchedule> FindPrimaryAccrualSchedules(IPrimaryAccrualScheduleStore store, PrimaryAccrualScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
