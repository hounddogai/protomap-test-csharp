namespace Acme.Workforce.Workplace.Occupancy.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy preferences by code.</summary>
public interface IOccupancyPreferenceStore
{
    OccupancyPreference? Find(string code);
    void Save(OccupancyPreference item);
    bool Remove(string code);
    List<OccupancyPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyPreferenceStore(Dictionary<string, OccupancyPreference> items, List<string> order) : IOccupancyPreferenceStore
{
    public Dictionary<string, OccupancyPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyPreference item)
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

    public List<OccupancyPreference> List()
    {
        var outValue = new List<OccupancyPreference>();
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
public sealed class CachedOccupancyPreferenceStore(MemoryOccupancyPreferenceStore inner, OccupancyPreferenceCache cache, int hits, int misses) : IOccupancyPreferenceStore
{
    public MemoryOccupancyPreferenceStore Inner { get; set; } = inner;
    public OccupancyPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyPreference? Find(string code)
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

    public void Save(OccupancyPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy preferences. Empty fields match all.</summary>
public sealed record OccupancyPreferenceQuery(string Country, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyPreference item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyPreference> Apply(List<OccupancyPreference> items)
    {
        var matched = Collections.Filter(items, (OccupancyPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy preference store.</summary>
public static class OccupancyPreferenceStore
{
    public static IOccupancyPreferenceStore NewMemoryOccupancyPreferenceStore()
    {
        return new MemoryOccupancyPreferenceStore(new Dictionary<string, OccupancyPreference>(), new List<string>());
    }

    public static IOccupancyPreferenceStore NewCachedOccupancyPreferenceStore(MemoryOccupancyPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyPreferenceStore(inner, OccupancyPreferenceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<OccupancyPreference> FindOccupancyPreferences(IOccupancyPreferenceStore store, OccupancyPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
