namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy pulse survey checkpoints by code.</summary>
public interface ILegacyPulseSurveyCheckpointStore
{
    LegacyPulseSurveyCheckpoint? Find(string code);
    void Save(LegacyPulseSurveyCheckpoint item);
    bool Remove(string code);
    List<LegacyPulseSurveyCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyPulseSurveyCheckpointStore(Dictionary<string, LegacyPulseSurveyCheckpoint> items, List<string> order) : ILegacyPulseSurveyCheckpointStore
{
    public Dictionary<string, LegacyPulseSurveyCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyPulseSurveyCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyPulseSurveyCheckpoint item)
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

    public List<LegacyPulseSurveyCheckpoint> List()
    {
        var outValue = new List<LegacyPulseSurveyCheckpoint>();
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
public sealed class CachedLegacyPulseSurveyCheckpointStore(MemoryLegacyPulseSurveyCheckpointStore inner, LegacyPulseSurveyCheckpointCache cache, int hits, int misses) : ILegacyPulseSurveyCheckpointStore
{
    public MemoryLegacyPulseSurveyCheckpointStore Inner { get; set; } = inner;
    public LegacyPulseSurveyCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyPulseSurveyCheckpoint? Find(string code)
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

    public void Save(LegacyPulseSurveyCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyPulseSurveyCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy pulse survey checkpoints. Empty fields match all.</summary>
public sealed record LegacyPulseSurveyCheckpointQuery(string Category, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyPulseSurveyCheckpoint item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<LegacyPulseSurveyCheckpoint> Apply(List<LegacyPulseSurveyCheckpoint> items)
    {
        var matched = Collections.Filter(items, (LegacyPulseSurveyCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy pulse survey checkpoint store.</summary>
public static class LegacyPulseSurveyCheckpointStore
{
    public static ILegacyPulseSurveyCheckpointStore NewMemoryLegacyPulseSurveyCheckpointStore()
    {
        return new MemoryLegacyPulseSurveyCheckpointStore(
            new Dictionary<string, LegacyPulseSurveyCheckpoint>(),
            new List<string>());
    }

    public static ILegacyPulseSurveyCheckpointStore NewCachedLegacyPulseSurveyCheckpointStore(MemoryLegacyPulseSurveyCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyPulseSurveyCheckpointStore(
            inner,
            LegacyPulseSurveyCheckpointCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LegacyPulseSurveyCheckpoint> FindLegacyPulseSurveyCheckpoints(ILegacyPulseSurveyCheckpointStore store, LegacyPulseSurveyCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
