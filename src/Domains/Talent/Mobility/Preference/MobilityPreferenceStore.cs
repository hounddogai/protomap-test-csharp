namespace Acme.Workforce.Talent.Mobility.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility preferences by code.</summary>
public interface IMobilityPreferenceStore
{
    MobilityPreference? Find(string code);
    void Save(MobilityPreference item);
    bool Remove(string code);
    List<MobilityPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityPreferenceStore(Dictionary<string, MobilityPreference> items, List<string> order) : IMobilityPreferenceStore
{
    public Dictionary<string, MobilityPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityPreference item)
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

    public List<MobilityPreference> List()
    {
        var outValue = new List<MobilityPreference>();
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
public sealed class CachedMobilityPreferenceStore(MemoryMobilityPreferenceStore inner, MobilityPreferenceCache cache, int hits, int misses) : IMobilityPreferenceStore
{
    public MemoryMobilityPreferenceStore Inner { get; set; } = inner;
    public MobilityPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityPreference? Find(string code)
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

    public void Save(MobilityPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility preferences. Empty fields match all.</summary>
public sealed record MobilityPreferenceQuery(string Segment, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityPreference item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MobilityPreference> Apply(List<MobilityPreference> items)
    {
        var matched = Collections.Filter(items, (MobilityPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility preference store.</summary>
public static class MobilityPreferenceStore
{
    public static IMobilityPreferenceStore NewMemoryMobilityPreferenceStore()
    {
        return new MemoryMobilityPreferenceStore(new Dictionary<string, MobilityPreference>(), new List<string>());
    }

    public static IMobilityPreferenceStore NewCachedMobilityPreferenceStore(MemoryMobilityPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityPreferenceStore(inner, MobilityPreferenceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<MobilityPreference> FindMobilityPreferences(IMobilityPreferenceStore store, MobilityPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
