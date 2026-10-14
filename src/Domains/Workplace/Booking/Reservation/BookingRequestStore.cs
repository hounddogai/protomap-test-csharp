namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking requests by code.</summary>
public interface IBookingRequestStore
{
    BookingRequest? Find(string code);
    void Save(BookingRequest item);
    bool Remove(string code);
    List<BookingRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingRequestStore(Dictionary<string, BookingRequest> items, List<string> order) : IBookingRequestStore
{
    public Dictionary<string, BookingRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingRequest item)
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

    public List<BookingRequest> List()
    {
        var outValue = new List<BookingRequest>();
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
public sealed class CachedBookingRequestStore(MemoryBookingRequestStore inner, BookingRequestCache cache, int hits, int misses) : IBookingRequestStore
{
    public MemoryBookingRequestStore Inner { get; set; } = inner;
    public BookingRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingRequest? Find(string code)
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

    public void Save(BookingRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking requests. Empty fields match all.</summary>
public sealed record BookingRequestQuery(string Title, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(BookingRequest item)
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

    public List<BookingRequest> Apply(List<BookingRequest> items)
    {
        var matched = Collections.Filter(items, (BookingRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking request store.</summary>
public static class BookingRequestStore
{
    public static IBookingRequestStore NewMemoryBookingRequestStore()
    {
        return new MemoryBookingRequestStore(new Dictionary<string, BookingRequest>(), new List<string>());
    }

    public static IBookingRequestStore NewCachedBookingRequestStore(MemoryBookingRequestStore inner, int ttl, IClock clock)
    {
        return new CachedBookingRequestStore(inner, BookingRequestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BookingRequest> FindBookingRequests(IBookingRequestStore store, BookingRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
