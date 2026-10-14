namespace Acme.Workforce.Contractors.Vendor.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled vendor schedules by code.</summary>
public interface IScheduledVendorScheduleStore
{
    ScheduledVendorSchedule? Find(string code);
    void Save(ScheduledVendorSchedule item);
    bool Remove(string code);
    List<ScheduledVendorSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledVendorScheduleStore(Dictionary<string, ScheduledVendorSchedule> items, List<string> order) : IScheduledVendorScheduleStore
{
    public Dictionary<string, ScheduledVendorSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledVendorSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledVendorSchedule item)
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

    public List<ScheduledVendorSchedule> List()
    {
        var outValue = new List<ScheduledVendorSchedule>();
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
public sealed class CachedScheduledVendorScheduleStore(MemoryScheduledVendorScheduleStore inner, ScheduledVendorScheduleCache cache, int hits, int misses) : IScheduledVendorScheduleStore
{
    public MemoryScheduledVendorScheduleStore Inner { get; set; } = inner;
    public ScheduledVendorScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledVendorSchedule? Find(string code)
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

    public void Save(ScheduledVendorSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledVendorSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled vendor schedules. Empty fields match all.</summary>
public sealed record ScheduledVendorScheduleQuery(string Title, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledVendorSchedule item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledVendorSchedule> Apply(List<ScheduledVendorSchedule> items)
    {
        var matched = Collections.Filter(items, (ScheduledVendorSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled vendor schedule store.</summary>
public static class ScheduledVendorScheduleStore
{
    public static IScheduledVendorScheduleStore NewMemoryScheduledVendorScheduleStore()
    {
        return new MemoryScheduledVendorScheduleStore(
            new Dictionary<string, ScheduledVendorSchedule>(),
            new List<string>());
    }

    public static IScheduledVendorScheduleStore NewCachedScheduledVendorScheduleStore(MemoryScheduledVendorScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledVendorScheduleStore(
            inner,
            ScheduledVendorScheduleCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledVendorSchedule> FindScheduledVendorSchedules(IScheduledVendorScheduleStore store, ScheduledVendorScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
