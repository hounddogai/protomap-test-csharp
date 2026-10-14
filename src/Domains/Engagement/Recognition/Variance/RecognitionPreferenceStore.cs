namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition preferences by code.</summary>
public interface IRecognitionPreferenceStore
{
    RecognitionPreference? Find(string code);
    void Save(RecognitionPreference item);
    bool Remove(string code);
    List<RecognitionPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionPreferenceStore(Dictionary<string, RecognitionPreference> items, List<string> order) : IRecognitionPreferenceStore
{
    public Dictionary<string, RecognitionPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionPreference item)
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

    public List<RecognitionPreference> List()
    {
        var outValue = new List<RecognitionPreference>();
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
public sealed class CachedRecognitionPreferenceStore(MemoryRecognitionPreferenceStore inner, RecognitionPreferenceCache cache, int hits, int misses) : IRecognitionPreferenceStore
{
    public MemoryRecognitionPreferenceStore Inner { get; set; } = inner;
    public RecognitionPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionPreference? Find(string code)
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

    public void Save(RecognitionPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition preferences. Empty fields match all.</summary>
public sealed record RecognitionPreferenceQuery(string Unit, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionPreference item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<RecognitionPreference> Apply(List<RecognitionPreference> items)
    {
        var matched = Collections.Filter(items, (RecognitionPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition preference store.</summary>
public static class RecognitionPreferenceStore
{
    public static IRecognitionPreferenceStore NewMemoryRecognitionPreferenceStore()
    {
        return new MemoryRecognitionPreferenceStore(
            new Dictionary<string, RecognitionPreference>(),
            new List<string>());
    }

    public static IRecognitionPreferenceStore NewCachedRecognitionPreferenceStore(MemoryRecognitionPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionPreferenceStore(inner, RecognitionPreferenceCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RecognitionPreference> FindRecognitionPreferences(IRecognitionPreferenceStore store, RecognitionPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
