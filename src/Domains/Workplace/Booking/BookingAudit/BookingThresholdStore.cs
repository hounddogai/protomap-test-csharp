namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking thresholds by code.</summary>
public interface IBookingThresholdStore
{
    BookingThreshold? Find(string code);
    void Save(BookingThreshold item);
    bool Remove(string code);
    List<BookingThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingThresholdStore(Dictionary<string, BookingThreshold> items, List<string> order) : IBookingThresholdStore
{
    public Dictionary<string, BookingThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingThreshold item)
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

    public List<BookingThreshold> List()
    {
        var outValue = new List<BookingThreshold>();
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
public sealed class CachedBookingThresholdStore(MemoryBookingThresholdStore inner, BookingThresholdCache cache, int hits, int misses) : IBookingThresholdStore
{
    public MemoryBookingThresholdStore Inner { get; set; } = inner;
    public BookingThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingThreshold? Find(string code)
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

    public void Save(BookingThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking thresholds. Empty fields match all.</summary>
public sealed record BookingThresholdQuery(string Label, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(BookingThreshold item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingThreshold> Apply(List<BookingThreshold> items)
    {
        var matched = Collections.Filter(items, (BookingThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking threshold store.</summary>
public static class BookingThresholdStore
{
    public static IBookingThresholdStore NewMemoryBookingThresholdStore()
    {
        return new MemoryBookingThresholdStore(new Dictionary<string, BookingThreshold>(), new List<string>());
    }

    public static IBookingThresholdStore NewCachedBookingThresholdStore(MemoryBookingThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedBookingThresholdStore(inner, BookingThresholdCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BookingThreshold> FindBookingThresholds(IBookingThresholdStore store, BookingThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
