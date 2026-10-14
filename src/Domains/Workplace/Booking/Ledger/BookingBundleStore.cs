namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking bundles by code.</summary>
public interface IBookingBundleStore
{
    BookingBundle? Find(string code);
    void Save(BookingBundle item);
    bool Remove(string code);
    List<BookingBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingBundleStore(Dictionary<string, BookingBundle> items, List<string> order) : IBookingBundleStore
{
    public Dictionary<string, BookingBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingBundle item)
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

    public List<BookingBundle> List()
    {
        var outValue = new List<BookingBundle>();
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
public sealed class CachedBookingBundleStore(MemoryBookingBundleStore inner, BookingBundleCache cache, int hits, int misses) : IBookingBundleStore
{
    public MemoryBookingBundleStore Inner { get; set; } = inner;
    public BookingBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingBundle? Find(string code)
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

    public void Save(BookingBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking bundles. Empty fields match all.</summary>
public sealed record BookingBundleQuery(string Source, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(BookingBundle item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingBundle> Apply(List<BookingBundle> items)
    {
        var matched = Collections.Filter(items, (BookingBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking bundle store.</summary>
public static class BookingBundleStore
{
    public static IBookingBundleStore NewMemoryBookingBundleStore()
    {
        return new MemoryBookingBundleStore(new Dictionary<string, BookingBundle>(), new List<string>());
    }

    public static IBookingBundleStore NewCachedBookingBundleStore(MemoryBookingBundleStore inner, int ttl, IClock clock)
    {
        return new CachedBookingBundleStore(inner, BookingBundleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BookingBundle> FindBookingBundles(IBookingBundleStore store, BookingBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
