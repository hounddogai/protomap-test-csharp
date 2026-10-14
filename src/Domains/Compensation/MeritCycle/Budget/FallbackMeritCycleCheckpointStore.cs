namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback merit cycle checkpoints by code.</summary>
public interface IFallbackMeritCycleCheckpointStore
{
    FallbackMeritCycleCheckpoint? Find(string code);
    void Save(FallbackMeritCycleCheckpoint item);
    bool Remove(string code);
    List<FallbackMeritCycleCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackMeritCycleCheckpointStore(Dictionary<string, FallbackMeritCycleCheckpoint> items, List<string> order) : IFallbackMeritCycleCheckpointStore
{
    public Dictionary<string, FallbackMeritCycleCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackMeritCycleCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackMeritCycleCheckpoint item)
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

    public List<FallbackMeritCycleCheckpoint> List()
    {
        var outValue = new List<FallbackMeritCycleCheckpoint>();
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
public sealed class CachedFallbackMeritCycleCheckpointStore(MemoryFallbackMeritCycleCheckpointStore inner, FallbackMeritCycleCheckpointCache cache, int hits, int misses) : IFallbackMeritCycleCheckpointStore
{
    public MemoryFallbackMeritCycleCheckpointStore Inner { get; set; } = inner;
    public FallbackMeritCycleCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackMeritCycleCheckpoint? Find(string code)
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

    public void Save(FallbackMeritCycleCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackMeritCycleCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback merit cycle checkpoints. Empty fields match all.</summary>
public sealed record FallbackMeritCycleCheckpointQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackMeritCycleCheckpoint item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackMeritCycleCheckpoint> Apply(List<FallbackMeritCycleCheckpoint> items)
    {
        var matched = Collections.Filter(items, (FallbackMeritCycleCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback merit cycle checkpoint store.</summary>
public static class FallbackMeritCycleCheckpointStore
{
    public static IFallbackMeritCycleCheckpointStore NewMemoryFallbackMeritCycleCheckpointStore()
    {
        return new MemoryFallbackMeritCycleCheckpointStore(
            new Dictionary<string, FallbackMeritCycleCheckpoint>(),
            new List<string>());
    }

    public static IFallbackMeritCycleCheckpointStore NewCachedFallbackMeritCycleCheckpointStore(MemoryFallbackMeritCycleCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackMeritCycleCheckpointStore(
            inner,
            FallbackMeritCycleCheckpointCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackMeritCycleCheckpoint> FindFallbackMeritCycleCheckpoints(IFallbackMeritCycleCheckpointStore store, FallbackMeritCycleCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
