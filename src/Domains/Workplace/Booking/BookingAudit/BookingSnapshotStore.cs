namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking snapshots by code.</summary>
public interface IBookingSnapshotStore
{
    BookingSnapshot? Find(string code);
    void Save(BookingSnapshot item);
    bool Remove(string code);
    List<BookingSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingSnapshotStore(Dictionary<string, BookingSnapshot> items, List<string> order) : IBookingSnapshotStore
{
    public Dictionary<string, BookingSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingSnapshot item)
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

    public List<BookingSnapshot> List()
    {
        var outValue = new List<BookingSnapshot>();
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
public sealed class CachedBookingSnapshotStore(MemoryBookingSnapshotStore inner, BookingSnapshotCache cache, int hits, int misses) : IBookingSnapshotStore
{
    public MemoryBookingSnapshotStore Inner { get; set; } = inner;
    public BookingSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingSnapshot? Find(string code)
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

    public void Save(BookingSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking snapshots. Empty fields match all.</summary>
public sealed record BookingSnapshotQuery(string Name, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(BookingSnapshot item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingSnapshot> Apply(List<BookingSnapshot> items)
    {
        var matched = Collections.Filter(items, (BookingSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking snapshot store.</summary>
public static class BookingSnapshotStore
{
    public static IBookingSnapshotStore NewMemoryBookingSnapshotStore()
    {
        return new MemoryBookingSnapshotStore(new Dictionary<string, BookingSnapshot>(), new List<string>());
    }

    public static IBookingSnapshotStore NewCachedBookingSnapshotStore(MemoryBookingSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedBookingSnapshotStore(inner, BookingSnapshotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BookingSnapshot> FindBookingSnapshots(IBookingSnapshotStore store, BookingSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
