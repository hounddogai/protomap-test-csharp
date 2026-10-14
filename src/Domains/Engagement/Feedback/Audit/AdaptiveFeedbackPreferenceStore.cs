namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive feedback preferences by code.</summary>
public interface IAdaptiveFeedbackPreferenceStore
{
    AdaptiveFeedbackPreference? Find(string code);
    void Save(AdaptiveFeedbackPreference item);
    bool Remove(string code);
    List<AdaptiveFeedbackPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveFeedbackPreferenceStore(Dictionary<string, AdaptiveFeedbackPreference> items, List<string> order) : IAdaptiveFeedbackPreferenceStore
{
    public Dictionary<string, AdaptiveFeedbackPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveFeedbackPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveFeedbackPreference item)
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

    public List<AdaptiveFeedbackPreference> List()
    {
        var outValue = new List<AdaptiveFeedbackPreference>();
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
public sealed class CachedAdaptiveFeedbackPreferenceStore(MemoryAdaptiveFeedbackPreferenceStore inner, AdaptiveFeedbackPreferenceCache cache, int hits, int misses) : IAdaptiveFeedbackPreferenceStore
{
    public MemoryAdaptiveFeedbackPreferenceStore Inner { get; set; } = inner;
    public AdaptiveFeedbackPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveFeedbackPreference? Find(string code)
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

    public void Save(AdaptiveFeedbackPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveFeedbackPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive feedback preferences. Empty fields match all.</summary>
public sealed record AdaptiveFeedbackPreferenceQuery(string Team, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveFeedbackPreference item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveFeedbackPreference> Apply(List<AdaptiveFeedbackPreference> items)
    {
        var matched = Collections.Filter(items, (AdaptiveFeedbackPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive feedback preference store.</summary>
public static class AdaptiveFeedbackPreferenceStore
{
    public static IAdaptiveFeedbackPreferenceStore NewMemoryAdaptiveFeedbackPreferenceStore()
    {
        return new MemoryAdaptiveFeedbackPreferenceStore(
            new Dictionary<string, AdaptiveFeedbackPreference>(),
            new List<string>());
    }

    public static IAdaptiveFeedbackPreferenceStore NewCachedAdaptiveFeedbackPreferenceStore(MemoryAdaptiveFeedbackPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveFeedbackPreferenceStore(
            inner,
            AdaptiveFeedbackPreferenceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveFeedbackPreference> FindAdaptiveFeedbackPreferences(IAdaptiveFeedbackPreferenceStore store, AdaptiveFeedbackPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
