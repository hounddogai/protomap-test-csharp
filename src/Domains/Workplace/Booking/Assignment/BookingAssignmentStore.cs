namespace Acme.Workforce.Workplace.Booking.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking assignments by code.</summary>
public interface IBookingAssignmentStore
{
    BookingAssignment? Find(string code);
    void Save(BookingAssignment item);
    bool Remove(string code);
    List<BookingAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingAssignmentStore(Dictionary<string, BookingAssignment> items, List<string> order) : IBookingAssignmentStore
{
    public Dictionary<string, BookingAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingAssignment item)
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

    public List<BookingAssignment> List()
    {
        var outValue = new List<BookingAssignment>();
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
public sealed class CachedBookingAssignmentStore(MemoryBookingAssignmentStore inner, BookingAssignmentCache cache, int hits, int misses) : IBookingAssignmentStore
{
    public MemoryBookingAssignmentStore Inner { get; set; } = inner;
    public BookingAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingAssignment? Find(string code)
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

    public void Save(BookingAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking assignments. Empty fields match all.</summary>
public sealed record BookingAssignmentQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(BookingAssignment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingAssignment> Apply(List<BookingAssignment> items)
    {
        var matched = Collections.Filter(items, (BookingAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking assignment store.</summary>
public static class BookingAssignmentStore
{
    public static IBookingAssignmentStore NewMemoryBookingAssignmentStore()
    {
        return new MemoryBookingAssignmentStore(new Dictionary<string, BookingAssignment>(), new List<string>());
    }

    public static IBookingAssignmentStore NewCachedBookingAssignmentStore(MemoryBookingAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedBookingAssignmentStore(inner, BookingAssignmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BookingAssignment> FindBookingAssignments(IBookingAssignmentStore store, BookingAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
