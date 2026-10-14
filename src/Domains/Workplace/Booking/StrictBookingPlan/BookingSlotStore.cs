namespace Acme.Workforce.Workplace.Booking.StrictBookingPlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking slots by code.</summary>
public interface IBookingSlotStore
{
    BookingSlot? Find(string code);
    void Save(BookingSlot item);
    bool Remove(string code);
    List<BookingSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingSlotStore(Dictionary<string, BookingSlot> items, List<string> order) : IBookingSlotStore
{
    public Dictionary<string, BookingSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingSlot item)
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

    public List<BookingSlot> List()
    {
        var outValue = new List<BookingSlot>();
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
public sealed class CachedBookingSlotStore(MemoryBookingSlotStore inner, BookingSlotCache cache, int hits, int misses) : IBookingSlotStore
{
    public MemoryBookingSlotStore Inner { get; set; } = inner;
    public BookingSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingSlot? Find(string code)
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

    public void Save(BookingSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking slots. Empty fields match all.</summary>
public sealed record BookingSlotQuery(string Channel, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(BookingSlot item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<BookingSlot> Apply(List<BookingSlot> items)
    {
        var matched = Collections.Filter(items, (BookingSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking slot store.</summary>
public static class BookingSlotStore
{
    public static IBookingSlotStore NewMemoryBookingSlotStore()
    {
        return new MemoryBookingSlotStore(new Dictionary<string, BookingSlot>(), new List<string>());
    }

    public static IBookingSlotStore NewCachedBookingSlotStore(MemoryBookingSlotStore inner, int ttl, IClock clock)
    {
        return new CachedBookingSlotStore(inner, BookingSlotCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BookingSlot> FindBookingSlots(IBookingSlotStore store, BookingSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
