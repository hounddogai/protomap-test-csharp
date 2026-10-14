namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due preferences by code.</summary>
public interface ITrainingDuePreferenceStore
{
    TrainingDuePreference? Find(string code);
    void Save(TrainingDuePreference item);
    bool Remove(string code);
    List<TrainingDuePreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDuePreferenceStore(Dictionary<string, TrainingDuePreference> items, List<string> order) : ITrainingDuePreferenceStore
{
    public Dictionary<string, TrainingDuePreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDuePreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDuePreference item)
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

    public List<TrainingDuePreference> List()
    {
        var outValue = new List<TrainingDuePreference>();
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
public sealed class CachedTrainingDuePreferenceStore(MemoryTrainingDuePreferenceStore inner, TrainingDuePreferenceCache cache, int hits, int misses) : ITrainingDuePreferenceStore
{
    public MemoryTrainingDuePreferenceStore Inner { get; set; } = inner;
    public TrainingDuePreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDuePreference? Find(string code)
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

    public void Save(TrainingDuePreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDuePreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due preferences. Empty fields match all.</summary>
public sealed record TrainingDuePreferenceQuery(string Title, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDuePreference item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDuePreference> Apply(List<TrainingDuePreference> items)
    {
        var matched = Collections.Filter(items, (TrainingDuePreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due preference store.</summary>
public static class TrainingDuePreferenceStore
{
    public static ITrainingDuePreferenceStore NewMemoryTrainingDuePreferenceStore()
    {
        return new MemoryTrainingDuePreferenceStore(
            new Dictionary<string, TrainingDuePreference>(),
            new List<string>());
    }

    public static ITrainingDuePreferenceStore NewCachedTrainingDuePreferenceStore(MemoryTrainingDuePreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDuePreferenceStore(inner, TrainingDuePreferenceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TrainingDuePreference> FindTrainingDuePreferences(ITrainingDuePreferenceStore store, TrainingDuePreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
