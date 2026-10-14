namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey preferences by code.</summary>
public interface IPulseSurveyPreferenceStore
{
    PulseSurveyPreference? Find(string code);
    void Save(PulseSurveyPreference item);
    bool Remove(string code);
    List<PulseSurveyPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyPreferenceStore(Dictionary<string, PulseSurveyPreference> items, List<string> order) : IPulseSurveyPreferenceStore
{
    public Dictionary<string, PulseSurveyPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyPreference item)
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

    public List<PulseSurveyPreference> List()
    {
        var outValue = new List<PulseSurveyPreference>();
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
public sealed class CachedPulseSurveyPreferenceStore(MemoryPulseSurveyPreferenceStore inner, PulseSurveyPreferenceCache cache, int hits, int misses) : IPulseSurveyPreferenceStore
{
    public MemoryPulseSurveyPreferenceStore Inner { get; set; } = inner;
    public PulseSurveyPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyPreference? Find(string code)
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

    public void Save(PulseSurveyPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey preferences. Empty fields match all.</summary>
public sealed record PulseSurveyPreferenceQuery(string Segment, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyPreference item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveyPreference> Apply(List<PulseSurveyPreference> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey preference store.</summary>
public static class PulseSurveyPreferenceStore
{
    public static IPulseSurveyPreferenceStore NewMemoryPulseSurveyPreferenceStore()
    {
        return new MemoryPulseSurveyPreferenceStore(
            new Dictionary<string, PulseSurveyPreference>(),
            new List<string>());
    }

    public static IPulseSurveyPreferenceStore NewCachedPulseSurveyPreferenceStore(MemoryPulseSurveyPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyPreferenceStore(inner, PulseSurveyPreferenceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyPreference> FindPulseSurveyPreferences(IPulseSurveyPreferenceStore store, PulseSurveyPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
