namespace Acme.Workforce.Absence.Holiday.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday entries by code.</summary>
public interface IHolidayEntryStore
{
    HolidayEntry? Find(string code);
    void Save(HolidayEntry item);
    bool Remove(string code);
    List<HolidayEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayEntryStore(Dictionary<string, HolidayEntry> items, List<string> order) : IHolidayEntryStore
{
    public Dictionary<string, HolidayEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayEntry item)
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

    public List<HolidayEntry> List()
    {
        var outValue = new List<HolidayEntry>();
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
public sealed class CachedHolidayEntryStore(MemoryHolidayEntryStore inner, HolidayEntryCache cache, int hits, int misses) : IHolidayEntryStore
{
    public MemoryHolidayEntryStore Inner { get; set; } = inner;
    public HolidayEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayEntry? Find(string code)
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

    public void Save(HolidayEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday entries. Empty fields match all.</summary>
public sealed record HolidayEntryQuery(string Title, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayEntry item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayEntry> Apply(List<HolidayEntry> items)
    {
        var matched = Collections.Filter(items, (HolidayEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday entry store.</summary>
public static class HolidayEntryStore
{
    public static IHolidayEntryStore NewMemoryHolidayEntryStore()
    {
        return new MemoryHolidayEntryStore(new Dictionary<string, HolidayEntry>(), new List<string>());
    }

    public static IHolidayEntryStore NewCachedHolidayEntryStore(MemoryHolidayEntryStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayEntryStore(inner, HolidayEntryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<HolidayEntry> FindHolidayEntries(IHolidayEntryStore store, HolidayEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
