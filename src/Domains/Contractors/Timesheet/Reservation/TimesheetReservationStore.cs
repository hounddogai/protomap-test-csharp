namespace Acme.Workforce.Contractors.Timesheet.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet reservations by code.</summary>
public interface ITimesheetReservationStore
{
    TimesheetReservation? Find(string code);
    void Save(TimesheetReservation item);
    bool Remove(string code);
    List<TimesheetReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetReservationStore(Dictionary<string, TimesheetReservation> items, List<string> order) : ITimesheetReservationStore
{
    public Dictionary<string, TimesheetReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetReservation item)
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

    public List<TimesheetReservation> List()
    {
        var outValue = new List<TimesheetReservation>();
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
public sealed class CachedTimesheetReservationStore(MemoryTimesheetReservationStore inner, TimesheetReservationCache cache, int hits, int misses) : ITimesheetReservationStore
{
    public MemoryTimesheetReservationStore Inner { get; set; } = inner;
    public TimesheetReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetReservation? Find(string code)
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

    public void Save(TimesheetReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet reservations. Empty fields match all.</summary>
public sealed record TimesheetReservationQuery(string Label, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetReservation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetReservation> Apply(List<TimesheetReservation> items)
    {
        var matched = Collections.Filter(items, (TimesheetReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet reservation store.</summary>
public static class TimesheetReservationStore
{
    public static ITimesheetReservationStore NewMemoryTimesheetReservationStore()
    {
        return new MemoryTimesheetReservationStore(
            new Dictionary<string, TimesheetReservation>(),
            new List<string>());
    }

    public static ITimesheetReservationStore NewCachedTimesheetReservationStore(MemoryTimesheetReservationStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetReservationStore(inner, TimesheetReservationCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TimesheetReservation> FindTimesheetReservations(ITimesheetReservationStore store, TimesheetReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
