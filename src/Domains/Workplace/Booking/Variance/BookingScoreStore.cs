namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking scores by code.</summary>
public interface IBookingScoreStore
{
    BookingScore? Find(string code);
    void Save(BookingScore item);
    bool Remove(string code);
    List<BookingScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingScoreStore(Dictionary<string, BookingScore> items, List<string> order) : IBookingScoreStore
{
    public Dictionary<string, BookingScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingScore item)
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

    public List<BookingScore> List()
    {
        var outValue = new List<BookingScore>();
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
public sealed class CachedBookingScoreStore(MemoryBookingScoreStore inner, BookingScoreCache cache, int hits, int misses) : IBookingScoreStore
{
    public MemoryBookingScoreStore Inner { get; set; } = inner;
    public BookingScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingScore? Find(string code)
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

    public void Save(BookingScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking scores. Empty fields match all.</summary>
public sealed record BookingScoreQuery(string Label, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(BookingScore item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingScore> Apply(List<BookingScore> items)
    {
        var matched = Collections.Filter(items, (BookingScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking score store.</summary>
public static class BookingScoreStore
{
    public static IBookingScoreStore NewMemoryBookingScoreStore()
    {
        return new MemoryBookingScoreStore(new Dictionary<string, BookingScore>(), new List<string>());
    }

    public static IBookingScoreStore NewCachedBookingScoreStore(MemoryBookingScoreStore inner, int ttl, IClock clock)
    {
        return new CachedBookingScoreStore(inner, BookingScoreCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BookingScore> FindBookingScores(IBookingScoreStore store, BookingScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
