namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking audits by code.</summary>
public interface IBookingAuditStore
{
    BookingAudit? Find(string code);
    void Save(BookingAudit item);
    bool Remove(string code);
    List<BookingAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingAuditStore(Dictionary<string, BookingAudit> items, List<string> order) : IBookingAuditStore
{
    public Dictionary<string, BookingAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingAudit item)
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

    public List<BookingAudit> List()
    {
        var outValue = new List<BookingAudit>();
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
public sealed class CachedBookingAuditStore(MemoryBookingAuditStore inner, BookingAuditCache cache, int hits, int misses) : IBookingAuditStore
{
    public MemoryBookingAuditStore Inner { get; set; } = inner;
    public BookingAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingAudit? Find(string code)
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

    public void Save(BookingAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking audits. Empty fields match all.</summary>
public sealed record BookingAuditQuery(string Name, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(BookingAudit item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingAudit> Apply(List<BookingAudit> items)
    {
        var matched = Collections.Filter(items, (BookingAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking audit store.</summary>
public static class BookingAuditStore
{
    public static IBookingAuditStore NewMemoryBookingAuditStore()
    {
        return new MemoryBookingAuditStore(new Dictionary<string, BookingAudit>(), new List<string>());
    }

    public static IBookingAuditStore NewCachedBookingAuditStore(MemoryBookingAuditStore inner, int ttl, IClock clock)
    {
        return new CachedBookingAuditStore(inner, BookingAuditCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BookingAudit> FindBookingAudits(IBookingAuditStore store, BookingAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
