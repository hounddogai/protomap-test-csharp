namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday reservations by code.</summary>
public interface IHolidayReservationStore
{
    HolidayReservation? Find(string code);
    void Save(HolidayReservation item);
    bool Remove(string code);
    List<HolidayReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayReservationStore(Dictionary<string, HolidayReservation> items, List<string> order) : IHolidayReservationStore
{
    public Dictionary<string, HolidayReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayReservation item)
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

    public List<HolidayReservation> List()
    {
        var outValue = new List<HolidayReservation>();
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
public sealed class CachedHolidayReservationStore(MemoryHolidayReservationStore inner, HolidayReservationCache cache, int hits, int misses) : IHolidayReservationStore
{
    public MemoryHolidayReservationStore Inner { get; set; } = inner;
    public HolidayReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayReservation? Find(string code)
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

    public void Save(HolidayReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday reservations. Empty fields match all.</summary>
public sealed record HolidayReservationQuery(string Locale, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayReservation item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayReservation> Apply(List<HolidayReservation> items)
    {
        var matched = Collections.Filter(items, (HolidayReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday reservation store.</summary>
public static class HolidayReservationStore
{
    public static IHolidayReservationStore NewMemoryHolidayReservationStore()
    {
        return new MemoryHolidayReservationStore(new Dictionary<string, HolidayReservation>(), new List<string>());
    }

    public static IHolidayReservationStore NewCachedHolidayReservationStore(MemoryHolidayReservationStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayReservationStore(inner, HolidayReservationCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HolidayReservation> FindHolidayReservations(IHolidayReservationStore store, HolidayReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
