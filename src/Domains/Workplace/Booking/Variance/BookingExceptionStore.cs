namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking exceptions by code.</summary>
public interface IBookingExceptionStore
{
    BookingException? Find(string code);
    void Save(BookingException item);
    bool Remove(string code);
    List<BookingException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingExceptionStore(Dictionary<string, BookingException> items, List<string> order) : IBookingExceptionStore
{
    public Dictionary<string, BookingException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingException item)
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

    public List<BookingException> List()
    {
        var outValue = new List<BookingException>();
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
public sealed class CachedBookingExceptionStore(MemoryBookingExceptionStore inner, BookingExceptionCache cache, int hits, int misses) : IBookingExceptionStore
{
    public MemoryBookingExceptionStore Inner { get; set; } = inner;
    public BookingExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingException? Find(string code)
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

    public void Save(BookingException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking exceptions. Empty fields match all.</summary>
public sealed record BookingExceptionQuery(string Label, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(BookingException item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<BookingException> Apply(List<BookingException> items)
    {
        var matched = Collections.Filter(items, (BookingException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking exception store.</summary>
public static class BookingExceptionStore
{
    public static IBookingExceptionStore NewMemoryBookingExceptionStore()
    {
        return new MemoryBookingExceptionStore(new Dictionary<string, BookingException>(), new List<string>());
    }

    public static IBookingExceptionStore NewCachedBookingExceptionStore(MemoryBookingExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedBookingExceptionStore(inner, BookingExceptionCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BookingException> FindBookingExceptions(IBookingExceptionStore store, BookingExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
