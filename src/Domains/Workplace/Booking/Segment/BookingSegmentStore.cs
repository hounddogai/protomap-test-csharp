namespace Acme.Workforce.Workplace.Booking.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking segments by code.</summary>
public interface IBookingSegmentStore
{
    BookingSegment? Find(string code);
    void Save(BookingSegment item);
    bool Remove(string code);
    List<BookingSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingSegmentStore(Dictionary<string, BookingSegment> items, List<string> order) : IBookingSegmentStore
{
    public Dictionary<string, BookingSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingSegment item)
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

    public List<BookingSegment> List()
    {
        var outValue = new List<BookingSegment>();
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
public sealed class CachedBookingSegmentStore(MemoryBookingSegmentStore inner, BookingSegmentCache cache, int hits, int misses) : IBookingSegmentStore
{
    public MemoryBookingSegmentStore Inner { get; set; } = inner;
    public BookingSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingSegment? Find(string code)
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

    public void Save(BookingSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking segments. Empty fields match all.</summary>
public sealed record BookingSegmentQuery(string Name, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(BookingSegment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingSegment> Apply(List<BookingSegment> items)
    {
        var matched = Collections.Filter(items, (BookingSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking segment store.</summary>
public static class BookingSegmentStore
{
    public static IBookingSegmentStore NewMemoryBookingSegmentStore()
    {
        return new MemoryBookingSegmentStore(new Dictionary<string, BookingSegment>(), new List<string>());
    }

    public static IBookingSegmentStore NewCachedBookingSegmentStore(MemoryBookingSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedBookingSegmentStore(inner, BookingSegmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BookingSegment> FindBookingSegments(IBookingSegmentStore store, BookingSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
