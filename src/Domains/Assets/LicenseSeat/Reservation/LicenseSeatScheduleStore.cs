namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat schedules by code.</summary>
public interface ILicenseSeatScheduleStore
{
    LicenseSeatSchedule? Find(string code);
    void Save(LicenseSeatSchedule item);
    bool Remove(string code);
    List<LicenseSeatSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatScheduleStore(Dictionary<string, LicenseSeatSchedule> items, List<string> order) : ILicenseSeatScheduleStore
{
    public Dictionary<string, LicenseSeatSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatSchedule item)
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

    public List<LicenseSeatSchedule> List()
    {
        var outValue = new List<LicenseSeatSchedule>();
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
public sealed class CachedLicenseSeatScheduleStore(MemoryLicenseSeatScheduleStore inner, LicenseSeatScheduleCache cache, int hits, int misses) : ILicenseSeatScheduleStore
{
    public MemoryLicenseSeatScheduleStore Inner { get; set; } = inner;
    public LicenseSeatScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatSchedule? Find(string code)
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

    public void Save(LicenseSeatSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat schedules. Empty fields match all.</summary>
public sealed record LicenseSeatScheduleQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatSchedule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<LicenseSeatSchedule> Apply(List<LicenseSeatSchedule> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat schedule store.</summary>
public static class LicenseSeatScheduleStore
{
    public static ILicenseSeatScheduleStore NewMemoryLicenseSeatScheduleStore()
    {
        return new MemoryLicenseSeatScheduleStore(new Dictionary<string, LicenseSeatSchedule>(), new List<string>());
    }

    public static ILicenseSeatScheduleStore NewCachedLicenseSeatScheduleStore(MemoryLicenseSeatScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatScheduleStore(inner, LicenseSeatScheduleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatSchedule> FindLicenseSeatSchedules(ILicenseSeatScheduleStore store, LicenseSeatScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
