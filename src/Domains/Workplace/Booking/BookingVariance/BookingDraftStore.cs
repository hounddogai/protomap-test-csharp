namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking drafts by code.</summary>
public interface IBookingDraftStore
{
    BookingDraft? Find(string code);
    void Save(BookingDraft item);
    bool Remove(string code);
    List<BookingDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingDraftStore(Dictionary<string, BookingDraft> items, List<string> order) : IBookingDraftStore
{
    public Dictionary<string, BookingDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingDraft item)
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

    public List<BookingDraft> List()
    {
        var outValue = new List<BookingDraft>();
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
public sealed class CachedBookingDraftStore(MemoryBookingDraftStore inner, BookingDraftCache cache, int hits, int misses) : IBookingDraftStore
{
    public MemoryBookingDraftStore Inner { get; set; } = inner;
    public BookingDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingDraft? Find(string code)
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

    public void Save(BookingDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking drafts. Empty fields match all.</summary>
public sealed record BookingDraftQuery(string Title, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(BookingDraft item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingDraft> Apply(List<BookingDraft> items)
    {
        var matched = Collections.Filter(items, (BookingDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking draft store.</summary>
public static class BookingDraftStore
{
    public static IBookingDraftStore NewMemoryBookingDraftStore()
    {
        return new MemoryBookingDraftStore(new Dictionary<string, BookingDraft>(), new List<string>());
    }

    public static IBookingDraftStore NewCachedBookingDraftStore(MemoryBookingDraftStore inner, int ttl, IClock clock)
    {
        return new CachedBookingDraftStore(inner, BookingDraftCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BookingDraft> FindBookingDrafts(IBookingDraftStore store, BookingDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
