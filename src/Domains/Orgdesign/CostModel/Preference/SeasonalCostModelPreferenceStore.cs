namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal cost model preferences by code.</summary>
public interface ISeasonalCostModelPreferenceStore
{
    SeasonalCostModelPreference? Find(string code);
    void Save(SeasonalCostModelPreference item);
    bool Remove(string code);
    List<SeasonalCostModelPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalCostModelPreferenceStore(Dictionary<string, SeasonalCostModelPreference> items, List<string> order) : ISeasonalCostModelPreferenceStore
{
    public Dictionary<string, SeasonalCostModelPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalCostModelPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalCostModelPreference item)
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

    public List<SeasonalCostModelPreference> List()
    {
        var outValue = new List<SeasonalCostModelPreference>();
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
public sealed class CachedSeasonalCostModelPreferenceStore(MemorySeasonalCostModelPreferenceStore inner, SeasonalCostModelPreferenceCache cache, int hits, int misses) : ISeasonalCostModelPreferenceStore
{
    public MemorySeasonalCostModelPreferenceStore Inner { get; set; } = inner;
    public SeasonalCostModelPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalCostModelPreference? Find(string code)
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

    public void Save(SeasonalCostModelPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalCostModelPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal cost model preferences. Empty fields match all.</summary>
public sealed record SeasonalCostModelPreferenceQuery(string Country, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalCostModelPreference item)
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

    public List<SeasonalCostModelPreference> Apply(List<SeasonalCostModelPreference> items)
    {
        var matched = Collections.Filter(items, (SeasonalCostModelPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal cost model preference store.</summary>
public static class SeasonalCostModelPreferenceStore
{
    public static ISeasonalCostModelPreferenceStore NewMemorySeasonalCostModelPreferenceStore()
    {
        return new MemorySeasonalCostModelPreferenceStore(
            new Dictionary<string, SeasonalCostModelPreference>(),
            new List<string>());
    }

    public static ISeasonalCostModelPreferenceStore NewCachedSeasonalCostModelPreferenceStore(MemorySeasonalCostModelPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalCostModelPreferenceStore(
            inner,
            SeasonalCostModelPreferenceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalCostModelPreference> FindSeasonalCostModelPreferences(ISeasonalCostModelPreferenceStore store, SeasonalCostModelPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
