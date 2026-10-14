namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking metrics by code.</summary>
public interface IBookingMetricStore
{
    BookingMetric? Find(string code);
    void Save(BookingMetric item);
    bool Remove(string code);
    List<BookingMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingMetricStore(Dictionary<string, BookingMetric> items, List<string> order) : IBookingMetricStore
{
    public Dictionary<string, BookingMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingMetric item)
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

    public List<BookingMetric> List()
    {
        var outValue = new List<BookingMetric>();
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
public sealed class CachedBookingMetricStore(MemoryBookingMetricStore inner, BookingMetricCache cache, int hits, int misses) : IBookingMetricStore
{
    public MemoryBookingMetricStore Inner { get; set; } = inner;
    public BookingMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingMetric? Find(string code)
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

    public void Save(BookingMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking metrics. Empty fields match all.</summary>
public sealed record BookingMetricQuery(string Category, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(BookingMetric item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingMetric> Apply(List<BookingMetric> items)
    {
        var matched = Collections.Filter(items, (BookingMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking metric store.</summary>
public static class BookingMetricStore
{
    public static IBookingMetricStore NewMemoryBookingMetricStore()
    {
        return new MemoryBookingMetricStore(new Dictionary<string, BookingMetric>(), new List<string>());
    }

    public static IBookingMetricStore NewCachedBookingMetricStore(MemoryBookingMetricStore inner, int ttl, IClock clock)
    {
        return new CachedBookingMetricStore(inner, BookingMetricCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BookingMetric> FindBookingMetrics(IBookingMetricStore store, BookingMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
