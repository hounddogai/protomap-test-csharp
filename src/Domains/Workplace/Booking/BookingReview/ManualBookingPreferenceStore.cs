namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual booking preferences by code.</summary>
public interface IManualBookingPreferenceStore
{
    ManualBookingPreference? Find(string code);
    void Save(ManualBookingPreference item);
    bool Remove(string code);
    List<ManualBookingPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualBookingPreferenceStore(Dictionary<string, ManualBookingPreference> items, List<string> order) : IManualBookingPreferenceStore
{
    public Dictionary<string, ManualBookingPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualBookingPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualBookingPreference item)
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

    public List<ManualBookingPreference> List()
    {
        var outValue = new List<ManualBookingPreference>();
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
public sealed class CachedManualBookingPreferenceStore(MemoryManualBookingPreferenceStore inner, ManualBookingPreferenceCache cache, int hits, int misses) : IManualBookingPreferenceStore
{
    public MemoryManualBookingPreferenceStore Inner { get; set; } = inner;
    public ManualBookingPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualBookingPreference? Find(string code)
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

    public void Save(ManualBookingPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualBookingPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual booking preferences. Empty fields match all.</summary>
public sealed record ManualBookingPreferenceQuery(string Country, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(ManualBookingPreference item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualBookingPreference> Apply(List<ManualBookingPreference> items)
    {
        var matched = Collections.Filter(items, (ManualBookingPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual booking preference store.</summary>
public static class ManualBookingPreferenceStore
{
    public static IManualBookingPreferenceStore NewMemoryManualBookingPreferenceStore()
    {
        return new MemoryManualBookingPreferenceStore(
            new Dictionary<string, ManualBookingPreference>(),
            new List<string>());
    }

    public static IManualBookingPreferenceStore NewCachedManualBookingPreferenceStore(MemoryManualBookingPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedManualBookingPreferenceStore(
            inner,
            ManualBookingPreferenceCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ManualBookingPreference> FindManualBookingPreferences(IManualBookingPreferenceStore store, ManualBookingPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
