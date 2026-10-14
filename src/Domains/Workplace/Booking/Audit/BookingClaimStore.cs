namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking claims by code.</summary>
public interface IBookingClaimStore
{
    BookingClaim? Find(string code);
    void Save(BookingClaim item);
    bool Remove(string code);
    List<BookingClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingClaimStore(Dictionary<string, BookingClaim> items, List<string> order) : IBookingClaimStore
{
    public Dictionary<string, BookingClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingClaim item)
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

    public List<BookingClaim> List()
    {
        var outValue = new List<BookingClaim>();
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
public sealed class CachedBookingClaimStore(MemoryBookingClaimStore inner, BookingClaimCache cache, int hits, int misses) : IBookingClaimStore
{
    public MemoryBookingClaimStore Inner { get; set; } = inner;
    public BookingClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingClaim? Find(string code)
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

    public void Save(BookingClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking claims. Empty fields match all.</summary>
public sealed record BookingClaimQuery(string Channel, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(BookingClaim item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingClaim> Apply(List<BookingClaim> items)
    {
        var matched = Collections.Filter(items, (BookingClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking claim store.</summary>
public static class BookingClaimStore
{
    public static IBookingClaimStore NewMemoryBookingClaimStore()
    {
        return new MemoryBookingClaimStore(new Dictionary<string, BookingClaim>(), new List<string>());
    }

    public static IBookingClaimStore NewCachedBookingClaimStore(MemoryBookingClaimStore inner, int ttl, IClock clock)
    {
        return new CachedBookingClaimStore(inner, BookingClaimCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BookingClaim> FindBookingClaims(IBookingClaimStore store, BookingClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
