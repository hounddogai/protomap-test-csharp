namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking plans by code.</summary>
public interface IBookingPlanStore
{
    BookingPlan? Find(string code);
    void Save(BookingPlan item);
    bool Remove(string code);
    List<BookingPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingPlanStore(Dictionary<string, BookingPlan> items, List<string> order) : IBookingPlanStore
{
    public Dictionary<string, BookingPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingPlan item)
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

    public List<BookingPlan> List()
    {
        var outValue = new List<BookingPlan>();
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
public sealed class CachedBookingPlanStore(MemoryBookingPlanStore inner, BookingPlanCache cache, int hits, int misses) : IBookingPlanStore
{
    public MemoryBookingPlanStore Inner { get; set; } = inner;
    public BookingPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingPlan? Find(string code)
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

    public void Save(BookingPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking plans. Empty fields match all.</summary>
public sealed record BookingPlanQuery(string Name, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(BookingPlan item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingPlan> Apply(List<BookingPlan> items)
    {
        var matched = Collections.Filter(items, (BookingPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking plan store.</summary>
public static class BookingPlanStore
{
    public static IBookingPlanStore NewMemoryBookingPlanStore()
    {
        return new MemoryBookingPlanStore(new Dictionary<string, BookingPlan>(), new List<string>());
    }

    public static IBookingPlanStore NewCachedBookingPlanStore(MemoryBookingPlanStore inner, int ttl, IClock clock)
    {
        return new CachedBookingPlanStore(inner, BookingPlanCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BookingPlan> FindBookingPlans(IBookingPlanStore store, BookingPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
