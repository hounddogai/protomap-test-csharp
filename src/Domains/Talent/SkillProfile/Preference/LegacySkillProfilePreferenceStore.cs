namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy skill profile preferences by code.</summary>
public interface ILegacySkillProfilePreferenceStore
{
    LegacySkillProfilePreference? Find(string code);
    void Save(LegacySkillProfilePreference item);
    bool Remove(string code);
    List<LegacySkillProfilePreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacySkillProfilePreferenceStore(Dictionary<string, LegacySkillProfilePreference> items, List<string> order) : ILegacySkillProfilePreferenceStore
{
    public Dictionary<string, LegacySkillProfilePreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacySkillProfilePreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacySkillProfilePreference item)
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

    public List<LegacySkillProfilePreference> List()
    {
        var outValue = new List<LegacySkillProfilePreference>();
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
public sealed class CachedLegacySkillProfilePreferenceStore(MemoryLegacySkillProfilePreferenceStore inner, LegacySkillProfilePreferenceCache cache, int hits, int misses) : ILegacySkillProfilePreferenceStore
{
    public MemoryLegacySkillProfilePreferenceStore Inner { get; set; } = inner;
    public LegacySkillProfilePreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacySkillProfilePreference? Find(string code)
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

    public void Save(LegacySkillProfilePreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacySkillProfilePreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy skill profile preferences. Empty fields match all.</summary>
public sealed record LegacySkillProfilePreferenceQuery(string Locale, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(LegacySkillProfilePreference item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<LegacySkillProfilePreference> Apply(List<LegacySkillProfilePreference> items)
    {
        var matched = Collections.Filter(items, (LegacySkillProfilePreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy skill profile preference store.</summary>
public static class LegacySkillProfilePreferenceStore
{
    public static ILegacySkillProfilePreferenceStore NewMemoryLegacySkillProfilePreferenceStore()
    {
        return new MemoryLegacySkillProfilePreferenceStore(
            new Dictionary<string, LegacySkillProfilePreference>(),
            new List<string>());
    }

    public static ILegacySkillProfilePreferenceStore NewCachedLegacySkillProfilePreferenceStore(MemoryLegacySkillProfilePreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedLegacySkillProfilePreferenceStore(
            inner,
            LegacySkillProfilePreferenceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LegacySkillProfilePreference> FindLegacySkillProfilePreferences(ILegacySkillProfilePreferenceStore store, LegacySkillProfilePreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
