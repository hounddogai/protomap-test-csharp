namespace Acme.Workforce.Workplace.Booking.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional booking preferences by code.</summary>
public interface IRegionalBookingPreferenceStore
{
    RegionalBookingPreference? Find(string code);
    void Save(RegionalBookingPreference item);
    bool Remove(string code);
    List<RegionalBookingPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalBookingPreferenceStore(Dictionary<string, RegionalBookingPreference> items, List<string> order) : IRegionalBookingPreferenceStore
{
    public Dictionary<string, RegionalBookingPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalBookingPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalBookingPreference item)
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

    public List<RegionalBookingPreference> List()
    {
        var outValue = new List<RegionalBookingPreference>();
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
public sealed class CachedRegionalBookingPreferenceStore(MemoryRegionalBookingPreferenceStore inner, RegionalBookingPreferenceCache cache, int hits, int misses) : IRegionalBookingPreferenceStore
{
    public MemoryRegionalBookingPreferenceStore Inner { get; set; } = inner;
    public RegionalBookingPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalBookingPreference? Find(string code)
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

    public void Save(RegionalBookingPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalBookingPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional booking preferences. Empty fields match all.</summary>
public sealed record RegionalBookingPreferenceQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalBookingPreference item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalBookingPreference> Apply(List<RegionalBookingPreference> items)
    {
        var matched = Collections.Filter(items, (RegionalBookingPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional booking preference store.</summary>
public static class RegionalBookingPreferenceStore
{
    public static IRegionalBookingPreferenceStore NewMemoryRegionalBookingPreferenceStore()
    {
        return new MemoryRegionalBookingPreferenceStore(
            new Dictionary<string, RegionalBookingPreference>(),
            new List<string>());
    }

    public static IRegionalBookingPreferenceStore NewCachedRegionalBookingPreferenceStore(MemoryRegionalBookingPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalBookingPreferenceStore(
            inner,
            RegionalBookingPreferenceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RegionalBookingPreference> FindRegionalBookingPreferences(IRegionalBookingPreferenceStore store, RegionalBookingPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
