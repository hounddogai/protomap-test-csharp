namespace Acme.Workforce.Compensation.Adjustment.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment checkpoints by code.</summary>
public interface IAdjustmentCheckpointStore
{
    AdjustmentCheckpoint? Find(string code);
    void Save(AdjustmentCheckpoint item);
    bool Remove(string code);
    List<AdjustmentCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentCheckpointStore(Dictionary<string, AdjustmentCheckpoint> items, List<string> order) : IAdjustmentCheckpointStore
{
    public Dictionary<string, AdjustmentCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentCheckpoint item)
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

    public List<AdjustmentCheckpoint> List()
    {
        var outValue = new List<AdjustmentCheckpoint>();
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
public sealed class CachedAdjustmentCheckpointStore(MemoryAdjustmentCheckpointStore inner, AdjustmentCheckpointCache cache, int hits, int misses) : IAdjustmentCheckpointStore
{
    public MemoryAdjustmentCheckpointStore Inner { get; set; } = inner;
    public AdjustmentCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentCheckpoint? Find(string code)
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

    public void Save(AdjustmentCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment checkpoints. Empty fields match all.</summary>
public sealed record AdjustmentCheckpointQuery(string Segment, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentCheckpoint item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentCheckpoint> Apply(List<AdjustmentCheckpoint> items)
    {
        var matched = Collections.Filter(items, (AdjustmentCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment checkpoint store.</summary>
public static class AdjustmentCheckpointStore
{
    public static IAdjustmentCheckpointStore NewMemoryAdjustmentCheckpointStore()
    {
        return new MemoryAdjustmentCheckpointStore(
            new Dictionary<string, AdjustmentCheckpoint>(),
            new List<string>());
    }

    public static IAdjustmentCheckpointStore NewCachedAdjustmentCheckpointStore(MemoryAdjustmentCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentCheckpointStore(inner, AdjustmentCheckpointCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AdjustmentCheckpoint> FindAdjustmentCheckpoints(IAdjustmentCheckpointStore store, AdjustmentCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
