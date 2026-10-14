namespace Acme.Workforce.Compliance.Attestation.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional attestation preferences by code.</summary>
public interface IRegionalAttestationPreferenceStore
{
    RegionalAttestationPreference? Find(string code);
    void Save(RegionalAttestationPreference item);
    bool Remove(string code);
    List<RegionalAttestationPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalAttestationPreferenceStore(Dictionary<string, RegionalAttestationPreference> items, List<string> order) : IRegionalAttestationPreferenceStore
{
    public Dictionary<string, RegionalAttestationPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalAttestationPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalAttestationPreference item)
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

    public List<RegionalAttestationPreference> List()
    {
        var outValue = new List<RegionalAttestationPreference>();
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
public sealed class CachedRegionalAttestationPreferenceStore(MemoryRegionalAttestationPreferenceStore inner, RegionalAttestationPreferenceCache cache, int hits, int misses) : IRegionalAttestationPreferenceStore
{
    public MemoryRegionalAttestationPreferenceStore Inner { get; set; } = inner;
    public RegionalAttestationPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalAttestationPreference? Find(string code)
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

    public void Save(RegionalAttestationPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalAttestationPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional attestation preferences. Empty fields match all.</summary>
public sealed record RegionalAttestationPreferenceQuery(string Title, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalAttestationPreference item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<RegionalAttestationPreference> Apply(List<RegionalAttestationPreference> items)
    {
        var matched = Collections.Filter(items, (RegionalAttestationPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional attestation preference store.</summary>
public static class RegionalAttestationPreferenceStore
{
    public static IRegionalAttestationPreferenceStore NewMemoryRegionalAttestationPreferenceStore()
    {
        return new MemoryRegionalAttestationPreferenceStore(
            new Dictionary<string, RegionalAttestationPreference>(),
            new List<string>());
    }

    public static IRegionalAttestationPreferenceStore NewCachedRegionalAttestationPreferenceStore(MemoryRegionalAttestationPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalAttestationPreferenceStore(
            inner,
            RegionalAttestationPreferenceCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalAttestationPreference> FindRegionalAttestationPreferences(IRegionalAttestationPreferenceStore store, RegionalAttestationPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
