namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking checkpoints by code.</summary>
public interface IBookingCheckpointStore
{
    BookingCheckpoint? Find(string code);
    void Save(BookingCheckpoint item);
    bool Remove(string code);
    List<BookingCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingCheckpointStore(Dictionary<string, BookingCheckpoint> items, List<string> order) : IBookingCheckpointStore
{
    public Dictionary<string, BookingCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingCheckpoint item)
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

    public List<BookingCheckpoint> List()
    {
        var outValue = new List<BookingCheckpoint>();
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
public sealed class CachedBookingCheckpointStore(MemoryBookingCheckpointStore inner, BookingCheckpointCache cache, int hits, int misses) : IBookingCheckpointStore
{
    public MemoryBookingCheckpointStore Inner { get; set; } = inner;
    public BookingCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingCheckpoint? Find(string code)
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

    public void Save(BookingCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking checkpoints. Empty fields match all.</summary>
public sealed record BookingCheckpointQuery(string Category, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(BookingCheckpoint item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<BookingCheckpoint> Apply(List<BookingCheckpoint> items)
    {
        var matched = Collections.Filter(items, (BookingCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking checkpoint store.</summary>
public static class BookingCheckpointStore
{
    public static IBookingCheckpointStore NewMemoryBookingCheckpointStore()
    {
        return new MemoryBookingCheckpointStore(new Dictionary<string, BookingCheckpoint>(), new List<string>());
    }

    public static IBookingCheckpointStore NewCachedBookingCheckpointStore(MemoryBookingCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedBookingCheckpointStore(inner, BookingCheckpointCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BookingCheckpoint> FindBookingCheckpoints(IBookingCheckpointStore store, BookingCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
