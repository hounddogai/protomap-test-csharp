namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional exception preferences by code.</summary>
public interface IRegionalExceptionPreferenceStore
{
    RegionalExceptionPreference? Find(string code);
    void Save(RegionalExceptionPreference item);
    bool Remove(string code);
    List<RegionalExceptionPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalExceptionPreferenceStore(Dictionary<string, RegionalExceptionPreference> items, List<string> order) : IRegionalExceptionPreferenceStore
{
    public Dictionary<string, RegionalExceptionPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalExceptionPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalExceptionPreference item)
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

    public List<RegionalExceptionPreference> List()
    {
        var outValue = new List<RegionalExceptionPreference>();
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
public sealed class CachedRegionalExceptionPreferenceStore(MemoryRegionalExceptionPreferenceStore inner, RegionalExceptionPreferenceCache cache, int hits, int misses) : IRegionalExceptionPreferenceStore
{
    public MemoryRegionalExceptionPreferenceStore Inner { get; set; } = inner;
    public RegionalExceptionPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalExceptionPreference? Find(string code)
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

    public void Save(RegionalExceptionPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalExceptionPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional exception preferences. Empty fields match all.</summary>
public sealed record RegionalExceptionPreferenceQuery(string Locale, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalExceptionPreference item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalExceptionPreference> Apply(List<RegionalExceptionPreference> items)
    {
        var matched = Collections.Filter(items, (RegionalExceptionPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional exception preference store.</summary>
public static class RegionalExceptionPreferenceStore
{
    public static IRegionalExceptionPreferenceStore NewMemoryRegionalExceptionPreferenceStore()
    {
        return new MemoryRegionalExceptionPreferenceStore(
            new Dictionary<string, RegionalExceptionPreference>(),
            new List<string>());
    }

    public static IRegionalExceptionPreferenceStore NewCachedRegionalExceptionPreferenceStore(MemoryRegionalExceptionPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalExceptionPreferenceStore(
            inner,
            RegionalExceptionPreferenceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RegionalExceptionPreference> FindRegionalExceptionPreferences(IRegionalExceptionPreferenceStore store, RegionalExceptionPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
