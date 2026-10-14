namespace Acme.Workforce.Workplace.Booking.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking digests by code.</summary>
public interface IBookingDigestStore
{
    BookingDigest? Find(string code);
    void Save(BookingDigest item);
    bool Remove(string code);
    List<BookingDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingDigestStore(Dictionary<string, BookingDigest> items, List<string> order) : IBookingDigestStore
{
    public Dictionary<string, BookingDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingDigest item)
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

    public List<BookingDigest> List()
    {
        var outValue = new List<BookingDigest>();
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
public sealed class CachedBookingDigestStore(MemoryBookingDigestStore inner, BookingDigestCache cache, int hits, int misses) : IBookingDigestStore
{
    public MemoryBookingDigestStore Inner { get; set; } = inner;
    public BookingDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingDigest? Find(string code)
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

    public void Save(BookingDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking digests. Empty fields match all.</summary>
public sealed record BookingDigestQuery(string Locale, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(BookingDigest item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingDigest> Apply(List<BookingDigest> items)
    {
        var matched = Collections.Filter(items, (BookingDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking digest store.</summary>
public static class BookingDigestStore
{
    public static IBookingDigestStore NewMemoryBookingDigestStore()
    {
        return new MemoryBookingDigestStore(new Dictionary<string, BookingDigest>(), new List<string>());
    }

    public static IBookingDigestStore NewCachedBookingDigestStore(MemoryBookingDigestStore inner, int ttl, IClock clock)
    {
        return new CachedBookingDigestStore(inner, BookingDigestCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BookingDigest> FindBookingDigests(IBookingDigestStore store, BookingDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
