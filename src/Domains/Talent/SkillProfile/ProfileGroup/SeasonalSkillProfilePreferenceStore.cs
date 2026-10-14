namespace Acme.Workforce.Talent.SkillProfile.ProfileGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal skill profile preferences by code.</summary>
public interface ISeasonalSkillProfilePreferenceStore
{
    SeasonalSkillProfilePreference? Find(string code);
    void Save(SeasonalSkillProfilePreference item);
    bool Remove(string code);
    List<SeasonalSkillProfilePreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalSkillProfilePreferenceStore(Dictionary<string, SeasonalSkillProfilePreference> items, List<string> order) : ISeasonalSkillProfilePreferenceStore
{
    public Dictionary<string, SeasonalSkillProfilePreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalSkillProfilePreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalSkillProfilePreference item)
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

    public List<SeasonalSkillProfilePreference> List()
    {
        var outValue = new List<SeasonalSkillProfilePreference>();
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
public sealed class CachedSeasonalSkillProfilePreferenceStore(MemorySeasonalSkillProfilePreferenceStore inner, SeasonalSkillProfilePreferenceCache cache, int hits, int misses) : ISeasonalSkillProfilePreferenceStore
{
    public MemorySeasonalSkillProfilePreferenceStore Inner { get; set; } = inner;
    public SeasonalSkillProfilePreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalSkillProfilePreference? Find(string code)
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

    public void Save(SeasonalSkillProfilePreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalSkillProfilePreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal skill profile preferences. Empty fields match all.</summary>
public sealed record SeasonalSkillProfilePreferenceQuery(string Locale, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalSkillProfilePreference item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalSkillProfilePreference> Apply(List<SeasonalSkillProfilePreference> items)
    {
        var matched = Collections.Filter(items, (SeasonalSkillProfilePreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal skill profile preference store.</summary>
public static class SeasonalSkillProfilePreferenceStore
{
    public static ISeasonalSkillProfilePreferenceStore NewMemorySeasonalSkillProfilePreferenceStore()
    {
        return new MemorySeasonalSkillProfilePreferenceStore(
            new Dictionary<string, SeasonalSkillProfilePreference>(),
            new List<string>());
    }

    public static ISeasonalSkillProfilePreferenceStore NewCachedSeasonalSkillProfilePreferenceStore(MemorySeasonalSkillProfilePreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalSkillProfilePreferenceStore(
            inner,
            SeasonalSkillProfilePreferenceCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalSkillProfilePreference> FindSeasonalSkillProfilePreferences(ISeasonalSkillProfilePreferenceStore store, SeasonalSkillProfilePreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
