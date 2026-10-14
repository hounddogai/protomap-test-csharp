namespace Acme.Workforce.Absence.Holiday.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday windows by code.</summary>
public interface IHolidayWindowStore
{
    HolidayWindow? Find(string code);
    void Save(HolidayWindow item);
    bool Remove(string code);
    List<HolidayWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayWindowStore(Dictionary<string, HolidayWindow> items, List<string> order) : IHolidayWindowStore
{
    public Dictionary<string, HolidayWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayWindow item)
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

    public List<HolidayWindow> List()
    {
        var outValue = new List<HolidayWindow>();
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
public sealed class CachedHolidayWindowStore(MemoryHolidayWindowStore inner, HolidayWindowCache cache, int hits, int misses) : IHolidayWindowStore
{
    public MemoryHolidayWindowStore Inner { get; set; } = inner;
    public HolidayWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayWindow? Find(string code)
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

    public void Save(HolidayWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday windows. Empty fields match all.</summary>
public sealed record HolidayWindowQuery(string Name, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayWindow item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayWindow> Apply(List<HolidayWindow> items)
    {
        var matched = Collections.Filter(items, (HolidayWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday window store.</summary>
public static class HolidayWindowStore
{
    public static IHolidayWindowStore NewMemoryHolidayWindowStore()
    {
        return new MemoryHolidayWindowStore(new Dictionary<string, HolidayWindow>(), new List<string>());
    }

    public static IHolidayWindowStore NewCachedHolidayWindowStore(MemoryHolidayWindowStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayWindowStore(inner, HolidayWindowCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<HolidayWindow> FindHolidayWindows(IHolidayWindowStore store, HolidayWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
