namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual leave request reservations by code.</summary>
public interface IManualLeaveRequestReservationStore
{
    ManualLeaveRequestReservation? Find(string code);
    void Save(ManualLeaveRequestReservation item);
    bool Remove(string code);
    List<ManualLeaveRequestReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualLeaveRequestReservationStore(Dictionary<string, ManualLeaveRequestReservation> items, List<string> order) : IManualLeaveRequestReservationStore
{
    public Dictionary<string, ManualLeaveRequestReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualLeaveRequestReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualLeaveRequestReservation item)
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

    public List<ManualLeaveRequestReservation> List()
    {
        var outValue = new List<ManualLeaveRequestReservation>();
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
public sealed class CachedManualLeaveRequestReservationStore(MemoryManualLeaveRequestReservationStore inner, ManualLeaveRequestReservationCache cache, int hits, int misses) : IManualLeaveRequestReservationStore
{
    public MemoryManualLeaveRequestReservationStore Inner { get; set; } = inner;
    public ManualLeaveRequestReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualLeaveRequestReservation? Find(string code)
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

    public void Save(ManualLeaveRequestReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualLeaveRequestReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual leave request reservations. Empty fields match all.</summary>
public sealed record ManualLeaveRequestReservationQuery(string Team, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualLeaveRequestReservation item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualLeaveRequestReservation> Apply(List<ManualLeaveRequestReservation> items)
    {
        var matched = Collections.Filter(items, (ManualLeaveRequestReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual leave request reservation store.</summary>
public static class ManualLeaveRequestReservationStore
{
    public static IManualLeaveRequestReservationStore NewMemoryManualLeaveRequestReservationStore()
    {
        return new MemoryManualLeaveRequestReservationStore(
            new Dictionary<string, ManualLeaveRequestReservation>(),
            new List<string>());
    }

    public static IManualLeaveRequestReservationStore NewCachedManualLeaveRequestReservationStore(MemoryManualLeaveRequestReservationStore inner, int ttl, IClock clock)
    {
        return new CachedManualLeaveRequestReservationStore(
            inner,
            ManualLeaveRequestReservationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ManualLeaveRequestReservation> FindManualLeaveRequestReservations(IManualLeaveRequestReservationStore store, ManualLeaveRequestReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
