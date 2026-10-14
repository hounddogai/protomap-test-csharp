namespace Acme.Workforce.Engagement.PulseSurvey.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global pulse survey preferences by code.</summary>
public interface IGlobalPulseSurveyPreferenceStore
{
    GlobalPulseSurveyPreference? Find(string code);
    void Save(GlobalPulseSurveyPreference item);
    bool Remove(string code);
    List<GlobalPulseSurveyPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPulseSurveyPreferenceStore(Dictionary<string, GlobalPulseSurveyPreference> items, List<string> order) : IGlobalPulseSurveyPreferenceStore
{
    public Dictionary<string, GlobalPulseSurveyPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPulseSurveyPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPulseSurveyPreference item)
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

    public List<GlobalPulseSurveyPreference> List()
    {
        var outValue = new List<GlobalPulseSurveyPreference>();
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
public sealed class CachedGlobalPulseSurveyPreferenceStore(MemoryGlobalPulseSurveyPreferenceStore inner, GlobalPulseSurveyPreferenceCache cache, int hits, int misses) : IGlobalPulseSurveyPreferenceStore
{
    public MemoryGlobalPulseSurveyPreferenceStore Inner { get; set; } = inner;
    public GlobalPulseSurveyPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPulseSurveyPreference? Find(string code)
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

    public void Save(GlobalPulseSurveyPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPulseSurveyPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global pulse survey preferences. Empty fields match all.</summary>
public sealed record GlobalPulseSurveyPreferenceQuery(string Source, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPulseSurveyPreference item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalPulseSurveyPreference> Apply(List<GlobalPulseSurveyPreference> items)
    {
        var matched = Collections.Filter(items, (GlobalPulseSurveyPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global pulse survey preference store.</summary>
public static class GlobalPulseSurveyPreferenceStore
{
    public static IGlobalPulseSurveyPreferenceStore NewMemoryGlobalPulseSurveyPreferenceStore()
    {
        return new MemoryGlobalPulseSurveyPreferenceStore(
            new Dictionary<string, GlobalPulseSurveyPreference>(),
            new List<string>());
    }

    public static IGlobalPulseSurveyPreferenceStore NewCachedGlobalPulseSurveyPreferenceStore(MemoryGlobalPulseSurveyPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPulseSurveyPreferenceStore(
            inner,
            GlobalPulseSurveyPreferenceCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<GlobalPulseSurveyPreference> FindGlobalPulseSurveyPreferences(IGlobalPulseSurveyPreferenceStore store, GlobalPulseSurveyPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
