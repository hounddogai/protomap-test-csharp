namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking profiles by code.</summary>
public interface IBookingProfileStore
{
    BookingProfile? Find(string code);
    void Save(BookingProfile item);
    bool Remove(string code);
    List<BookingProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingProfileStore(Dictionary<string, BookingProfile> items, List<string> order) : IBookingProfileStore
{
    public Dictionary<string, BookingProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingProfile item)
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

    public List<BookingProfile> List()
    {
        var outValue = new List<BookingProfile>();
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
public sealed class CachedBookingProfileStore(MemoryBookingProfileStore inner, BookingProfileCache cache, int hits, int misses) : IBookingProfileStore
{
    public MemoryBookingProfileStore Inner { get; set; } = inner;
    public BookingProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingProfile? Find(string code)
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

    public void Save(BookingProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking profiles. Empty fields match all.</summary>
public sealed record BookingProfileQuery(string Unit, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(BookingProfile item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<BookingProfile> Apply(List<BookingProfile> items)
    {
        var matched = Collections.Filter(items, (BookingProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking profile store.</summary>
public static class BookingProfileStore
{
    public static IBookingProfileStore NewMemoryBookingProfileStore()
    {
        return new MemoryBookingProfileStore(new Dictionary<string, BookingProfile>(), new List<string>());
    }

    public static IBookingProfileStore NewCachedBookingProfileStore(MemoryBookingProfileStore inner, int ttl, IClock clock)
    {
        return new CachedBookingProfileStore(inner, BookingProfileCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BookingProfile> FindBookingProfiles(IBookingProfileStore store, BookingProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
