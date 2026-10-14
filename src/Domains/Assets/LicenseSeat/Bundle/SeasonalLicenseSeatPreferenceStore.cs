namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal license seat preferences by code.</summary>
public interface ISeasonalLicenseSeatPreferenceStore
{
    SeasonalLicenseSeatPreference? Find(string code);
    void Save(SeasonalLicenseSeatPreference item);
    bool Remove(string code);
    List<SeasonalLicenseSeatPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalLicenseSeatPreferenceStore(Dictionary<string, SeasonalLicenseSeatPreference> items, List<string> order) : ISeasonalLicenseSeatPreferenceStore
{
    public Dictionary<string, SeasonalLicenseSeatPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalLicenseSeatPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalLicenseSeatPreference item)
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

    public List<SeasonalLicenseSeatPreference> List()
    {
        var outValue = new List<SeasonalLicenseSeatPreference>();
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
public sealed class CachedSeasonalLicenseSeatPreferenceStore(MemorySeasonalLicenseSeatPreferenceStore inner, SeasonalLicenseSeatPreferenceCache cache, int hits, int misses) : ISeasonalLicenseSeatPreferenceStore
{
    public MemorySeasonalLicenseSeatPreferenceStore Inner { get; set; } = inner;
    public SeasonalLicenseSeatPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalLicenseSeatPreference? Find(string code)
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

    public void Save(SeasonalLicenseSeatPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalLicenseSeatPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal license seat preferences. Empty fields match all.</summary>
public sealed record SeasonalLicenseSeatPreferenceQuery(string Region, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalLicenseSeatPreference item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<SeasonalLicenseSeatPreference> Apply(List<SeasonalLicenseSeatPreference> items)
    {
        var matched = Collections.Filter(items, (SeasonalLicenseSeatPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal license seat preference store.</summary>
public static class SeasonalLicenseSeatPreferenceStore
{
    public static ISeasonalLicenseSeatPreferenceStore NewMemorySeasonalLicenseSeatPreferenceStore()
    {
        return new MemorySeasonalLicenseSeatPreferenceStore(
            new Dictionary<string, SeasonalLicenseSeatPreference>(),
            new List<string>());
    }

    public static ISeasonalLicenseSeatPreferenceStore NewCachedSeasonalLicenseSeatPreferenceStore(MemorySeasonalLicenseSeatPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalLicenseSeatPreferenceStore(
            inner,
            SeasonalLicenseSeatPreferenceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalLicenseSeatPreference> FindSeasonalLicenseSeatPreferences(ISeasonalLicenseSeatPreferenceStore store, SeasonalLicenseSeatPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
