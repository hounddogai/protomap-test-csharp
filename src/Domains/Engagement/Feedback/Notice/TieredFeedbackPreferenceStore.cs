namespace Acme.Workforce.Engagement.Feedback.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered feedback preferences by code.</summary>
public interface ITieredFeedbackPreferenceStore
{
    TieredFeedbackPreference? Find(string code);
    void Save(TieredFeedbackPreference item);
    bool Remove(string code);
    List<TieredFeedbackPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredFeedbackPreferenceStore(Dictionary<string, TieredFeedbackPreference> items, List<string> order) : ITieredFeedbackPreferenceStore
{
    public Dictionary<string, TieredFeedbackPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredFeedbackPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredFeedbackPreference item)
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

    public List<TieredFeedbackPreference> List()
    {
        var outValue = new List<TieredFeedbackPreference>();
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
public sealed class CachedTieredFeedbackPreferenceStore(MemoryTieredFeedbackPreferenceStore inner, TieredFeedbackPreferenceCache cache, int hits, int misses) : ITieredFeedbackPreferenceStore
{
    public MemoryTieredFeedbackPreferenceStore Inner { get; set; } = inner;
    public TieredFeedbackPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredFeedbackPreference? Find(string code)
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

    public void Save(TieredFeedbackPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredFeedbackPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered feedback preferences. Empty fields match all.</summary>
public sealed record TieredFeedbackPreferenceQuery(string Locale, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(TieredFeedbackPreference item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredFeedbackPreference> Apply(List<TieredFeedbackPreference> items)
    {
        var matched = Collections.Filter(items, (TieredFeedbackPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered feedback preference store.</summary>
public static class TieredFeedbackPreferenceStore
{
    public static ITieredFeedbackPreferenceStore NewMemoryTieredFeedbackPreferenceStore()
    {
        return new MemoryTieredFeedbackPreferenceStore(
            new Dictionary<string, TieredFeedbackPreference>(),
            new List<string>());
    }

    public static ITieredFeedbackPreferenceStore NewCachedTieredFeedbackPreferenceStore(MemoryTieredFeedbackPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedTieredFeedbackPreferenceStore(
            inner,
            TieredFeedbackPreferenceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<TieredFeedbackPreference> FindTieredFeedbackPreferences(ITieredFeedbackPreferenceStore store, TieredFeedbackPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
