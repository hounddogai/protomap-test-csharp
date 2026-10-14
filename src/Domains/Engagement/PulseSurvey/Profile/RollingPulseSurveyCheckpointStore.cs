namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling pulse survey checkpoints by code.</summary>
public interface IRollingPulseSurveyCheckpointStore
{
    RollingPulseSurveyCheckpoint? Find(string code);
    void Save(RollingPulseSurveyCheckpoint item);
    bool Remove(string code);
    List<RollingPulseSurveyCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingPulseSurveyCheckpointStore(Dictionary<string, RollingPulseSurveyCheckpoint> items, List<string> order) : IRollingPulseSurveyCheckpointStore
{
    public Dictionary<string, RollingPulseSurveyCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingPulseSurveyCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingPulseSurveyCheckpoint item)
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

    public List<RollingPulseSurveyCheckpoint> List()
    {
        var outValue = new List<RollingPulseSurveyCheckpoint>();
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
public sealed class CachedRollingPulseSurveyCheckpointStore(MemoryRollingPulseSurveyCheckpointStore inner, RollingPulseSurveyCheckpointCache cache, int hits, int misses) : IRollingPulseSurveyCheckpointStore
{
    public MemoryRollingPulseSurveyCheckpointStore Inner { get; set; } = inner;
    public RollingPulseSurveyCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingPulseSurveyCheckpoint? Find(string code)
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

    public void Save(RollingPulseSurveyCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingPulseSurveyCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling pulse survey checkpoints. Empty fields match all.</summary>
public sealed record RollingPulseSurveyCheckpointQuery(string Locale, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(RollingPulseSurveyCheckpoint item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingPulseSurveyCheckpoint> Apply(List<RollingPulseSurveyCheckpoint> items)
    {
        var matched = Collections.Filter(items, (RollingPulseSurveyCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling pulse survey checkpoint store.</summary>
public static class RollingPulseSurveyCheckpointStore
{
    public static IRollingPulseSurveyCheckpointStore NewMemoryRollingPulseSurveyCheckpointStore()
    {
        return new MemoryRollingPulseSurveyCheckpointStore(
            new Dictionary<string, RollingPulseSurveyCheckpoint>(),
            new List<string>());
    }

    public static IRollingPulseSurveyCheckpointStore NewCachedRollingPulseSurveyCheckpointStore(MemoryRollingPulseSurveyCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedRollingPulseSurveyCheckpointStore(
            inner,
            RollingPulseSurveyCheckpointCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RollingPulseSurveyCheckpoint> FindRollingPulseSurveyCheckpoints(IRollingPulseSurveyCheckpointStore store, RollingPulseSurveyCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
