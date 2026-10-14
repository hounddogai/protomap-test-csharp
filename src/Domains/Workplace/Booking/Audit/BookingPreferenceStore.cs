namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking preferences by code.</summary>
public interface IBookingPreferenceStore
{
    BookingPreference? Find(string code);
    void Save(BookingPreference item);
    bool Remove(string code);
    List<BookingPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingPreferenceStore(Dictionary<string, BookingPreference> items, List<string> order) : IBookingPreferenceStore
{
    public Dictionary<string, BookingPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingPreference item)
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

    public List<BookingPreference> List()
    {
        var outValue = new List<BookingPreference>();
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
public sealed class CachedBookingPreferenceStore(MemoryBookingPreferenceStore inner, BookingPreferenceCache cache, int hits, int misses) : IBookingPreferenceStore
{
    public MemoryBookingPreferenceStore Inner { get; set; } = inner;
    public BookingPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingPreference? Find(string code)
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

    public void Save(BookingPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking preferences. Empty fields match all.</summary>
public sealed record BookingPreferenceQuery(string Label, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(BookingPreference item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingPreference> Apply(List<BookingPreference> items)
    {
        var matched = Collections.Filter(items, (BookingPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking preference store.</summary>
public static class BookingPreferenceStore
{
    public static IBookingPreferenceStore NewMemoryBookingPreferenceStore()
    {
        return new MemoryBookingPreferenceStore(new Dictionary<string, BookingPreference>(), new List<string>());
    }

    public static IBookingPreferenceStore NewCachedBookingPreferenceStore(MemoryBookingPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedBookingPreferenceStore(inner, BookingPreferenceCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BookingPreference> FindBookingPreferences(IBookingPreferenceStore store, BookingPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
