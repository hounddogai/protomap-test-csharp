namespace Acme.Workforce.Workplace.Booking.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking reviews by code.</summary>
public interface IBookingReviewStore
{
    BookingReview? Find(string code);
    void Save(BookingReview item);
    bool Remove(string code);
    List<BookingReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingReviewStore(Dictionary<string, BookingReview> items, List<string> order) : IBookingReviewStore
{
    public Dictionary<string, BookingReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingReview item)
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

    public List<BookingReview> List()
    {
        var outValue = new List<BookingReview>();
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
public sealed class CachedBookingReviewStore(MemoryBookingReviewStore inner, BookingReviewCache cache, int hits, int misses) : IBookingReviewStore
{
    public MemoryBookingReviewStore Inner { get; set; } = inner;
    public BookingReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingReview? Find(string code)
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

    public void Save(BookingReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking reviews. Empty fields match all.</summary>
public sealed record BookingReviewQuery(string Team, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(BookingReview item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingReview> Apply(List<BookingReview> items)
    {
        var matched = Collections.Filter(items, (BookingReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking review store.</summary>
public static class BookingReviewStore
{
    public static IBookingReviewStore NewMemoryBookingReviewStore()
    {
        return new MemoryBookingReviewStore(new Dictionary<string, BookingReview>(), new List<string>());
    }

    public static IBookingReviewStore NewCachedBookingReviewStore(MemoryBookingReviewStore inner, int ttl, IClock clock)
    {
        return new CachedBookingReviewStore(inner, BookingReviewCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BookingReview> FindBookingReviews(IBookingReviewStore store, BookingReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
