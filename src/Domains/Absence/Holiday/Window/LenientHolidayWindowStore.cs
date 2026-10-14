namespace Acme.Workforce.Absence.Holiday.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient holiday windows by code.</summary>
public interface ILenientHolidayWindowStore
{
    LenientHolidayWindow? Find(string code);
    void Save(LenientHolidayWindow item);
    bool Remove(string code);
    List<LenientHolidayWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientHolidayWindowStore(Dictionary<string, LenientHolidayWindow> items, List<string> order) : ILenientHolidayWindowStore
{
    public Dictionary<string, LenientHolidayWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientHolidayWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientHolidayWindow item)
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

    public List<LenientHolidayWindow> List()
    {
        var outValue = new List<LenientHolidayWindow>();
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
public sealed class CachedLenientHolidayWindowStore(MemoryLenientHolidayWindowStore inner, LenientHolidayWindowCache cache, int hits, int misses) : ILenientHolidayWindowStore
{
    public MemoryLenientHolidayWindowStore Inner { get; set; } = inner;
    public LenientHolidayWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientHolidayWindow? Find(string code)
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

    public void Save(LenientHolidayWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientHolidayWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient holiday windows. Empty fields match all.</summary>
public sealed record LenientHolidayWindowQuery(string Source, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(LenientHolidayWindow item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientHolidayWindow> Apply(List<LenientHolidayWindow> items)
    {
        var matched = Collections.Filter(items, (LenientHolidayWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient holiday window store.</summary>
public static class LenientHolidayWindowStore
{
    public static ILenientHolidayWindowStore NewMemoryLenientHolidayWindowStore()
    {
        return new MemoryLenientHolidayWindowStore(
            new Dictionary<string, LenientHolidayWindow>(),
            new List<string>());
    }

    public static ILenientHolidayWindowStore NewCachedLenientHolidayWindowStore(MemoryLenientHolidayWindowStore inner, int ttl, IClock clock)
    {
        return new CachedLenientHolidayWindowStore(inner, LenientHolidayWindowCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LenientHolidayWindow> FindLenientHolidayWindows(ILenientHolidayWindowStore store, LenientHolidayWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
