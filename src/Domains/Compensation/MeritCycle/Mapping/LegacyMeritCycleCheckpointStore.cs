namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy merit cycle checkpoints by code.</summary>
public interface ILegacyMeritCycleCheckpointStore
{
    LegacyMeritCycleCheckpoint? Find(string code);
    void Save(LegacyMeritCycleCheckpoint item);
    bool Remove(string code);
    List<LegacyMeritCycleCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyMeritCycleCheckpointStore(Dictionary<string, LegacyMeritCycleCheckpoint> items, List<string> order) : ILegacyMeritCycleCheckpointStore
{
    public Dictionary<string, LegacyMeritCycleCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyMeritCycleCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyMeritCycleCheckpoint item)
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

    public List<LegacyMeritCycleCheckpoint> List()
    {
        var outValue = new List<LegacyMeritCycleCheckpoint>();
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
public sealed class CachedLegacyMeritCycleCheckpointStore(MemoryLegacyMeritCycleCheckpointStore inner, LegacyMeritCycleCheckpointCache cache, int hits, int misses) : ILegacyMeritCycleCheckpointStore
{
    public MemoryLegacyMeritCycleCheckpointStore Inner { get; set; } = inner;
    public LegacyMeritCycleCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyMeritCycleCheckpoint? Find(string code)
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

    public void Save(LegacyMeritCycleCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyMeritCycleCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy merit cycle checkpoints. Empty fields match all.</summary>
public sealed record LegacyMeritCycleCheckpointQuery(string Region, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyMeritCycleCheckpoint item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyMeritCycleCheckpoint> Apply(List<LegacyMeritCycleCheckpoint> items)
    {
        var matched = Collections.Filter(items, (LegacyMeritCycleCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy merit cycle checkpoint store.</summary>
public static class LegacyMeritCycleCheckpointStore
{
    public static ILegacyMeritCycleCheckpointStore NewMemoryLegacyMeritCycleCheckpointStore()
    {
        return new MemoryLegacyMeritCycleCheckpointStore(
            new Dictionary<string, LegacyMeritCycleCheckpoint>(),
            new List<string>());
    }

    public static ILegacyMeritCycleCheckpointStore NewCachedLegacyMeritCycleCheckpointStore(MemoryLegacyMeritCycleCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyMeritCycleCheckpointStore(
            inner,
            LegacyMeritCycleCheckpointCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LegacyMeritCycleCheckpoint> FindLegacyMeritCycleCheckpoints(ILegacyMeritCycleCheckpointStore store, LegacyMeritCycleCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
