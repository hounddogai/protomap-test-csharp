namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey checkpoints by code.</summary>
public interface IPulseSurveyCheckpointStore
{
    PulseSurveyCheckpoint? Find(string code);
    void Save(PulseSurveyCheckpoint item);
    bool Remove(string code);
    List<PulseSurveyCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyCheckpointStore(Dictionary<string, PulseSurveyCheckpoint> items, List<string> order) : IPulseSurveyCheckpointStore
{
    public Dictionary<string, PulseSurveyCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyCheckpoint item)
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

    public List<PulseSurveyCheckpoint> List()
    {
        var outValue = new List<PulseSurveyCheckpoint>();
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
public sealed class CachedPulseSurveyCheckpointStore(MemoryPulseSurveyCheckpointStore inner, PulseSurveyCheckpointCache cache, int hits, int misses) : IPulseSurveyCheckpointStore
{
    public MemoryPulseSurveyCheckpointStore Inner { get; set; } = inner;
    public PulseSurveyCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyCheckpoint? Find(string code)
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

    public void Save(PulseSurveyCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey checkpoints. Empty fields match all.</summary>
public sealed record PulseSurveyCheckpointQuery(string Label, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyCheckpoint item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveyCheckpoint> Apply(List<PulseSurveyCheckpoint> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey checkpoint store.</summary>
public static class PulseSurveyCheckpointStore
{
    public static IPulseSurveyCheckpointStore NewMemoryPulseSurveyCheckpointStore()
    {
        return new MemoryPulseSurveyCheckpointStore(
            new Dictionary<string, PulseSurveyCheckpoint>(),
            new List<string>());
    }

    public static IPulseSurveyCheckpointStore NewCachedPulseSurveyCheckpointStore(MemoryPulseSurveyCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyCheckpointStore(
            inner,
            PulseSurveyCheckpointCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PulseSurveyCheckpoint> FindPulseSurveyCheckpoints(IPulseSurveyCheckpointStore store, PulseSurveyCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
