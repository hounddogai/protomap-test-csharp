namespace Acme.Workforce.Workplace.Booking.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking targets by code.</summary>
public interface IBookingTargetStore
{
    BookingTarget? Find(string code);
    void Save(BookingTarget item);
    bool Remove(string code);
    List<BookingTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingTargetStore(Dictionary<string, BookingTarget> items, List<string> order) : IBookingTargetStore
{
    public Dictionary<string, BookingTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingTarget item)
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

    public List<BookingTarget> List()
    {
        var outValue = new List<BookingTarget>();
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
public sealed class CachedBookingTargetStore(MemoryBookingTargetStore inner, BookingTargetCache cache, int hits, int misses) : IBookingTargetStore
{
    public MemoryBookingTargetStore Inner { get; set; } = inner;
    public BookingTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingTarget? Find(string code)
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

    public void Save(BookingTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking targets. Empty fields match all.</summary>
public sealed record BookingTargetQuery(string Region, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(BookingTarget item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingTarget> Apply(List<BookingTarget> items)
    {
        var matched = Collections.Filter(items, (BookingTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking target store.</summary>
public static class BookingTargetStore
{
    public static IBookingTargetStore NewMemoryBookingTargetStore()
    {
        return new MemoryBookingTargetStore(new Dictionary<string, BookingTarget>(), new List<string>());
    }

    public static IBookingTargetStore NewCachedBookingTargetStore(MemoryBookingTargetStore inner, int ttl, IClock clock)
    {
        return new CachedBookingTargetStore(inner, BookingTargetCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BookingTarget> FindBookingTargets(IBookingTargetStore store, BookingTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
