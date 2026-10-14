namespace Acme.Workforce.Absence.Holiday.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday bundles by code.</summary>
public interface IHolidayBundleStore
{
    HolidayBundle? Find(string code);
    void Save(HolidayBundle item);
    bool Remove(string code);
    List<HolidayBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayBundleStore(Dictionary<string, HolidayBundle> items, List<string> order) : IHolidayBundleStore
{
    public Dictionary<string, HolidayBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayBundle item)
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

    public List<HolidayBundle> List()
    {
        var outValue = new List<HolidayBundle>();
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
public sealed class CachedHolidayBundleStore(MemoryHolidayBundleStore inner, HolidayBundleCache cache, int hits, int misses) : IHolidayBundleStore
{
    public MemoryHolidayBundleStore Inner { get; set; } = inner;
    public HolidayBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayBundle? Find(string code)
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

    public void Save(HolidayBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday bundles. Empty fields match all.</summary>
public sealed record HolidayBundleQuery(string Country, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayBundle item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayBundle> Apply(List<HolidayBundle> items)
    {
        var matched = Collections.Filter(items, (HolidayBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday bundle store.</summary>
public static class HolidayBundleStore
{
    public static IHolidayBundleStore NewMemoryHolidayBundleStore()
    {
        return new MemoryHolidayBundleStore(new Dictionary<string, HolidayBundle>(), new List<string>());
    }

    public static IHolidayBundleStore NewCachedHolidayBundleStore(MemoryHolidayBundleStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayBundleStore(inner, HolidayBundleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<HolidayBundle> FindHolidayBundles(IHolidayBundleStore store, HolidayBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
