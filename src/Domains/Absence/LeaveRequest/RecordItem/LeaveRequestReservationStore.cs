namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request reservations by code.</summary>
public interface ILeaveRequestReservationStore
{
    LeaveRequestReservation? Find(string code);
    void Save(LeaveRequestReservation item);
    bool Remove(string code);
    List<LeaveRequestReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestReservationStore(Dictionary<string, LeaveRequestReservation> items, List<string> order) : ILeaveRequestReservationStore
{
    public Dictionary<string, LeaveRequestReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestReservation item)
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

    public List<LeaveRequestReservation> List()
    {
        var outValue = new List<LeaveRequestReservation>();
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
public sealed class CachedLeaveRequestReservationStore(MemoryLeaveRequestReservationStore inner, LeaveRequestReservationCache cache, int hits, int misses) : ILeaveRequestReservationStore
{
    public MemoryLeaveRequestReservationStore Inner { get; set; } = inner;
    public LeaveRequestReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestReservation? Find(string code)
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

    public void Save(LeaveRequestReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request reservations. Empty fields match all.</summary>
public sealed record LeaveRequestReservationQuery(string Name, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestReservation item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<LeaveRequestReservation> Apply(List<LeaveRequestReservation> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request reservation store.</summary>
public static class LeaveRequestReservationStore
{
    public static ILeaveRequestReservationStore NewMemoryLeaveRequestReservationStore()
    {
        return new MemoryLeaveRequestReservationStore(
            new Dictionary<string, LeaveRequestReservation>(),
            new List<string>());
    }

    public static ILeaveRequestReservationStore NewCachedLeaveRequestReservationStore(MemoryLeaveRequestReservationStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestReservationStore(
            inner,
            LeaveRequestReservationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LeaveRequestReservation> FindLeaveRequestReservations(ILeaveRequestReservationStore store, LeaveRequestReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
