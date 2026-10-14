namespace Acme.Workforce.Workplace.Booking.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking manifests by code.</summary>
public interface IBookingManifestStore
{
    BookingManifest? Find(string code);
    void Save(BookingManifest item);
    bool Remove(string code);
    List<BookingManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingManifestStore(Dictionary<string, BookingManifest> items, List<string> order) : IBookingManifestStore
{
    public Dictionary<string, BookingManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingManifest item)
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

    public List<BookingManifest> List()
    {
        var outValue = new List<BookingManifest>();
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
public sealed class CachedBookingManifestStore(MemoryBookingManifestStore inner, BookingManifestCache cache, int hits, int misses) : IBookingManifestStore
{
    public MemoryBookingManifestStore Inner { get; set; } = inner;
    public BookingManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingManifest? Find(string code)
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

    public void Save(BookingManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking manifests. Empty fields match all.</summary>
public sealed record BookingManifestQuery(string Department, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(BookingManifest item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingManifest> Apply(List<BookingManifest> items)
    {
        var matched = Collections.Filter(items, (BookingManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking manifest store.</summary>
public static class BookingManifestStore
{
    public static IBookingManifestStore NewMemoryBookingManifestStore()
    {
        return new MemoryBookingManifestStore(new Dictionary<string, BookingManifest>(), new List<string>());
    }

    public static IBookingManifestStore NewCachedBookingManifestStore(MemoryBookingManifestStore inner, int ttl, IClock clock)
    {
        return new CachedBookingManifestStore(inner, BookingManifestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BookingManifest> FindBookingManifests(IBookingManifestStore store, BookingManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
