namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient leave request reservations by code.</summary>
public interface ILenientLeaveRequestReservationStore
{
    LenientLeaveRequestReservation? Find(string code);
    void Save(LenientLeaveRequestReservation item);
    bool Remove(string code);
    List<LenientLeaveRequestReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientLeaveRequestReservationStore(Dictionary<string, LenientLeaveRequestReservation> items, List<string> order) : ILenientLeaveRequestReservationStore
{
    public Dictionary<string, LenientLeaveRequestReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientLeaveRequestReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientLeaveRequestReservation item)
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

    public List<LenientLeaveRequestReservation> List()
    {
        var outValue = new List<LenientLeaveRequestReservation>();
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
public sealed class CachedLenientLeaveRequestReservationStore(MemoryLenientLeaveRequestReservationStore inner, LenientLeaveRequestReservationCache cache, int hits, int misses) : ILenientLeaveRequestReservationStore
{
    public MemoryLenientLeaveRequestReservationStore Inner { get; set; } = inner;
    public LenientLeaveRequestReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientLeaveRequestReservation? Find(string code)
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

    public void Save(LenientLeaveRequestReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientLeaveRequestReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient leave request reservations. Empty fields match all.</summary>
public sealed record LenientLeaveRequestReservationQuery(string Title, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(LenientLeaveRequestReservation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientLeaveRequestReservation> Apply(List<LenientLeaveRequestReservation> items)
    {
        var matched = Collections.Filter(items, (LenientLeaveRequestReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient leave request reservation store.</summary>
public static class LenientLeaveRequestReservationStore
{
    public static ILenientLeaveRequestReservationStore NewMemoryLenientLeaveRequestReservationStore()
    {
        return new MemoryLenientLeaveRequestReservationStore(
            new Dictionary<string, LenientLeaveRequestReservation>(),
            new List<string>());
    }

    public static ILenientLeaveRequestReservationStore NewCachedLenientLeaveRequestReservationStore(MemoryLenientLeaveRequestReservationStore inner, int ttl, IClock clock)
    {
        return new CachedLenientLeaveRequestReservationStore(
            inner,
            LenientLeaveRequestReservationCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LenientLeaveRequestReservation> FindLenientLeaveRequestReservations(ILenientLeaveRequestReservationStore store, LenientLeaveRequestReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
