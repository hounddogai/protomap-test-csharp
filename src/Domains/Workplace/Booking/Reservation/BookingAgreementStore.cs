namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking agreements by code.</summary>
public interface IBookingAgreementStore
{
    BookingAgreement? Find(string code);
    void Save(BookingAgreement item);
    bool Remove(string code);
    List<BookingAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingAgreementStore(Dictionary<string, BookingAgreement> items, List<string> order) : IBookingAgreementStore
{
    public Dictionary<string, BookingAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingAgreement item)
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

    public List<BookingAgreement> List()
    {
        var outValue = new List<BookingAgreement>();
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
public sealed class CachedBookingAgreementStore(MemoryBookingAgreementStore inner, BookingAgreementCache cache, int hits, int misses) : IBookingAgreementStore
{
    public MemoryBookingAgreementStore Inner { get; set; } = inner;
    public BookingAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingAgreement? Find(string code)
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

    public void Save(BookingAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking agreements. Empty fields match all.</summary>
public sealed record BookingAgreementQuery(string Country, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(BookingAgreement item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingAgreement> Apply(List<BookingAgreement> items)
    {
        var matched = Collections.Filter(items, (BookingAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking agreement store.</summary>
public static class BookingAgreementStore
{
    public static IBookingAgreementStore NewMemoryBookingAgreementStore()
    {
        return new MemoryBookingAgreementStore(new Dictionary<string, BookingAgreement>(), new List<string>());
    }

    public static IBookingAgreementStore NewCachedBookingAgreementStore(MemoryBookingAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedBookingAgreementStore(inner, BookingAgreementCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BookingAgreement> FindBookingAgreements(IBookingAgreementStore store, BookingAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
