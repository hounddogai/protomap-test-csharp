namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary booking windows by code.</summary>
public interface IPrimaryBookingWindowStore
{
    PrimaryBookingWindow? Find(string code);
    void Save(PrimaryBookingWindow item);
    bool Remove(string code);
    List<PrimaryBookingWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryBookingWindowStore(Dictionary<string, PrimaryBookingWindow> items, List<string> order) : IPrimaryBookingWindowStore
{
    public Dictionary<string, PrimaryBookingWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryBookingWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryBookingWindow item)
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

    public List<PrimaryBookingWindow> List()
    {
        var outValue = new List<PrimaryBookingWindow>();
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
public sealed class CachedPrimaryBookingWindowStore(MemoryPrimaryBookingWindowStore inner, PrimaryBookingWindowCache cache, int hits, int misses) : IPrimaryBookingWindowStore
{
    public MemoryPrimaryBookingWindowStore Inner { get; set; } = inner;
    public PrimaryBookingWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryBookingWindow? Find(string code)
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

    public void Save(PrimaryBookingWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryBookingWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary booking windows. Empty fields match all.</summary>
public sealed record PrimaryBookingWindowQuery(string Department, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryBookingWindow item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryBookingWindow> Apply(List<PrimaryBookingWindow> items)
    {
        var matched = Collections.Filter(items, (PrimaryBookingWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary booking window store.</summary>
public static class PrimaryBookingWindowStore
{
    public static IPrimaryBookingWindowStore NewMemoryPrimaryBookingWindowStore()
    {
        return new MemoryPrimaryBookingWindowStore(
            new Dictionary<string, PrimaryBookingWindow>(),
            new List<string>());
    }

    public static IPrimaryBookingWindowStore NewCachedPrimaryBookingWindowStore(MemoryPrimaryBookingWindowStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryBookingWindowStore(inner, PrimaryBookingWindowCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PrimaryBookingWindow> FindPrimaryBookingWindows(IPrimaryBookingWindowStore store, PrimaryBookingWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
