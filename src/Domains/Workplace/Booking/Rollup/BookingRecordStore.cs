namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking records by code.</summary>
public interface IBookingRecordStore
{
    BookingRecord? Find(string code);
    void Save(BookingRecord item);
    bool Remove(string code);
    List<BookingRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingRecordStore(Dictionary<string, BookingRecord> items, List<string> order) : IBookingRecordStore
{
    public Dictionary<string, BookingRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingRecord item)
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

    public List<BookingRecord> List()
    {
        var outValue = new List<BookingRecord>();
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
public sealed class CachedBookingRecordStore(MemoryBookingRecordStore inner, BookingRecordCache cache, int hits, int misses) : IBookingRecordStore
{
    public MemoryBookingRecordStore Inner { get; set; } = inner;
    public BookingRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingRecord? Find(string code)
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

    public void Save(BookingRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking records. Empty fields match all.</summary>
public sealed record BookingRecordQuery(string Segment, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(BookingRecord item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingRecord> Apply(List<BookingRecord> items)
    {
        var matched = Collections.Filter(items, (BookingRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking record store.</summary>
public static class BookingRecordStore
{
    public static IBookingRecordStore NewMemoryBookingRecordStore()
    {
        return new MemoryBookingRecordStore(new Dictionary<string, BookingRecord>(), new List<string>());
    }

    public static IBookingRecordStore NewCachedBookingRecordStore(MemoryBookingRecordStore inner, int ttl, IClock clock)
    {
        return new CachedBookingRecordStore(inner, BookingRecordCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BookingRecord> FindBookingRecords(IBookingRecordStore store, BookingRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
