namespace Acme.Workforce.Workplace.Booking.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual booking entries by code.</summary>
public interface IManualBookingEntryStore
{
    ManualBookingEntry? Find(string code);
    void Save(ManualBookingEntry item);
    bool Remove(string code);
    List<ManualBookingEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualBookingEntryStore(Dictionary<string, ManualBookingEntry> items, List<string> order) : IManualBookingEntryStore
{
    public Dictionary<string, ManualBookingEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualBookingEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualBookingEntry item)
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

    public List<ManualBookingEntry> List()
    {
        var outValue = new List<ManualBookingEntry>();
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
public sealed class CachedManualBookingEntryStore(MemoryManualBookingEntryStore inner, ManualBookingEntryCache cache, int hits, int misses) : IManualBookingEntryStore
{
    public MemoryManualBookingEntryStore Inner { get; set; } = inner;
    public ManualBookingEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualBookingEntry? Find(string code)
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

    public void Save(ManualBookingEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualBookingEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual booking entries. Empty fields match all.</summary>
public sealed record ManualBookingEntryQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ManualBookingEntry item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualBookingEntry> Apply(List<ManualBookingEntry> items)
    {
        var matched = Collections.Filter(items, (ManualBookingEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual booking entry store.</summary>
public static class ManualBookingEntryStore
{
    public static IManualBookingEntryStore NewMemoryManualBookingEntryStore()
    {
        return new MemoryManualBookingEntryStore(new Dictionary<string, ManualBookingEntry>(), new List<string>());
    }

    public static IManualBookingEntryStore NewCachedManualBookingEntryStore(MemoryManualBookingEntryStore inner, int ttl, IClock clock)
    {
        return new CachedManualBookingEntryStore(inner, ManualBookingEntryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ManualBookingEntry> FindManualBookingEntries(IManualBookingEntryStore store, ManualBookingEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
