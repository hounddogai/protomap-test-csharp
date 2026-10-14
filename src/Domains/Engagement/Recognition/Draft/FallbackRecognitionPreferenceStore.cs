namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback recognition preferences by code.</summary>
public interface IFallbackRecognitionPreferenceStore
{
    FallbackRecognitionPreference? Find(string code);
    void Save(FallbackRecognitionPreference item);
    bool Remove(string code);
    List<FallbackRecognitionPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackRecognitionPreferenceStore(Dictionary<string, FallbackRecognitionPreference> items, List<string> order) : IFallbackRecognitionPreferenceStore
{
    public Dictionary<string, FallbackRecognitionPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackRecognitionPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackRecognitionPreference item)
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

    public List<FallbackRecognitionPreference> List()
    {
        var outValue = new List<FallbackRecognitionPreference>();
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
public sealed class CachedFallbackRecognitionPreferenceStore(MemoryFallbackRecognitionPreferenceStore inner, FallbackRecognitionPreferenceCache cache, int hits, int misses) : IFallbackRecognitionPreferenceStore
{
    public MemoryFallbackRecognitionPreferenceStore Inner { get; set; } = inner;
    public FallbackRecognitionPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackRecognitionPreference? Find(string code)
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

    public void Save(FallbackRecognitionPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackRecognitionPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback recognition preferences. Empty fields match all.</summary>
public sealed record FallbackRecognitionPreferenceQuery(string Country, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackRecognitionPreference item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackRecognitionPreference> Apply(List<FallbackRecognitionPreference> items)
    {
        var matched = Collections.Filter(items, (FallbackRecognitionPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback recognition preference store.</summary>
public static class FallbackRecognitionPreferenceStore
{
    public static IFallbackRecognitionPreferenceStore NewMemoryFallbackRecognitionPreferenceStore()
    {
        return new MemoryFallbackRecognitionPreferenceStore(
            new Dictionary<string, FallbackRecognitionPreference>(),
            new List<string>());
    }

    public static IFallbackRecognitionPreferenceStore NewCachedFallbackRecognitionPreferenceStore(MemoryFallbackRecognitionPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackRecognitionPreferenceStore(
            inner,
            FallbackRecognitionPreferenceCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<FallbackRecognitionPreference> FindFallbackRecognitionPreferences(IFallbackRecognitionPreferenceStore store, FallbackRecognitionPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
