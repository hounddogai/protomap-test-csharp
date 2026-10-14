namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered merit cycle checkpoints by code.</summary>
public interface ITieredMeritCycleCheckpointStore
{
    TieredMeritCycleCheckpoint? Find(string code);
    void Save(TieredMeritCycleCheckpoint item);
    bool Remove(string code);
    List<TieredMeritCycleCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredMeritCycleCheckpointStore(Dictionary<string, TieredMeritCycleCheckpoint> items, List<string> order) : ITieredMeritCycleCheckpointStore
{
    public Dictionary<string, TieredMeritCycleCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredMeritCycleCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredMeritCycleCheckpoint item)
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

    public List<TieredMeritCycleCheckpoint> List()
    {
        var outValue = new List<TieredMeritCycleCheckpoint>();
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
public sealed class CachedTieredMeritCycleCheckpointStore(MemoryTieredMeritCycleCheckpointStore inner, TieredMeritCycleCheckpointCache cache, int hits, int misses) : ITieredMeritCycleCheckpointStore
{
    public MemoryTieredMeritCycleCheckpointStore Inner { get; set; } = inner;
    public TieredMeritCycleCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredMeritCycleCheckpoint? Find(string code)
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

    public void Save(TieredMeritCycleCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredMeritCycleCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered merit cycle checkpoints. Empty fields match all.</summary>
public sealed record TieredMeritCycleCheckpointQuery(string Title, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(TieredMeritCycleCheckpoint item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredMeritCycleCheckpoint> Apply(List<TieredMeritCycleCheckpoint> items)
    {
        var matched = Collections.Filter(items, (TieredMeritCycleCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered merit cycle checkpoint store.</summary>
public static class TieredMeritCycleCheckpointStore
{
    public static ITieredMeritCycleCheckpointStore NewMemoryTieredMeritCycleCheckpointStore()
    {
        return new MemoryTieredMeritCycleCheckpointStore(
            new Dictionary<string, TieredMeritCycleCheckpoint>(),
            new List<string>());
    }

    public static ITieredMeritCycleCheckpointStore NewCachedTieredMeritCycleCheckpointStore(MemoryTieredMeritCycleCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedTieredMeritCycleCheckpointStore(
            inner,
            TieredMeritCycleCheckpointCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<TieredMeritCycleCheckpoint> FindTieredMeritCycleCheckpoints(ITieredMeritCycleCheckpointStore store, TieredMeritCycleCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
