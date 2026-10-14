namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps booking budgets by code.</summary>
public interface IBookingBudgetStore
{
    BookingBudget? Find(string code);
    void Save(BookingBudget item);
    bool Remove(string code);
    List<BookingBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBookingBudgetStore(Dictionary<string, BookingBudget> items, List<string> order) : IBookingBudgetStore
{
    public Dictionary<string, BookingBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BookingBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BookingBudget item)
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

    public List<BookingBudget> List()
    {
        var outValue = new List<BookingBudget>();
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
public sealed class CachedBookingBudgetStore(MemoryBookingBudgetStore inner, BookingBudgetCache cache, int hits, int misses) : IBookingBudgetStore
{
    public MemoryBookingBudgetStore Inner { get; set; } = inner;
    public BookingBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BookingBudget? Find(string code)
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

    public void Save(BookingBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BookingBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects booking budgets. Empty fields match all.</summary>
public sealed record BookingBudgetQuery(string Label, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(BookingBudget item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BookingBudget> Apply(List<BookingBudget> items)
    {
        var matched = Collections.Filter(items, (BookingBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of booking budget store.</summary>
public static class BookingBudgetStore
{
    public static IBookingBudgetStore NewMemoryBookingBudgetStore()
    {
        return new MemoryBookingBudgetStore(new Dictionary<string, BookingBudget>(), new List<string>());
    }

    public static IBookingBudgetStore NewCachedBookingBudgetStore(MemoryBookingBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedBookingBudgetStore(inner, BookingBudgetCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BookingBudget> FindBookingBudgets(IBookingBudgetStore store, BookingBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
