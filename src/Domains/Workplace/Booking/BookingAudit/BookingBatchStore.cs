namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking batches by code.</summary>
public interface IBookingBatchStore
{
    BookingBatch? Find(string code);
    void Save(BookingBatch item);
    bool Remove(string code);
    List<BookingBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingBatchStore(Dictionary<string, BookingBatch> items, List<string> order) : IBookingBatchStore
{
    public Dictionary<string, BookingBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingBatch item)
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

    public List<BookingBatch> List()
    {
        var outValue = new List<BookingBatch>();
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
public sealed class CachedBookingBatchStore(MemoryBookingBatchStore inner, BookingBatchCache cache, int hits, int misses) : IBookingBatchStore
{
    public MemoryBookingBatchStore Inner { get; set; } = inner;
    public BookingBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingBatch? Find(string code)
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

    public void Save(BookingBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking batches. Empty fields match all.</summary>
public sealed record BookingBatchQuery(string Label, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(BookingBatch item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingBatch> Apply(List<BookingBatch> items)
    {
        var matched = Collections.Filter(items, (BookingBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking batch store.</summary>
public static class BookingBatchStore
{
    public static IBookingBatchStore NewMemoryBookingBatchStore()
    {
        return new MemoryBookingBatchStore(new Dictionary<string, BookingBatch>(), new List<string>());
    }

    public static IBookingBatchStore NewCachedBookingBatchStore(MemoryBookingBatchStore inner, int ttl, IClock clock)
    {
        return new CachedBookingBatchStore(inner, BookingBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BookingBatch> FindBookingBatches(IBookingBatchStore store, BookingBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
