namespace Acme.Workforce.Contractors.Engagement.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending engagement checkpoints by code.</summary>
public interface IPendingEngagementCheckpointStore
{
    PendingEngagementCheckpoint? Find(string code);
    void Save(PendingEngagementCheckpoint item);
    bool Remove(string code);
    List<PendingEngagementCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingEngagementCheckpointStore(Dictionary<string, PendingEngagementCheckpoint> items, List<string> order) : IPendingEngagementCheckpointStore
{
    public Dictionary<string, PendingEngagementCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingEngagementCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingEngagementCheckpoint item)
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

    public List<PendingEngagementCheckpoint> List()
    {
        var outValue = new List<PendingEngagementCheckpoint>();
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
public sealed class CachedPendingEngagementCheckpointStore(MemoryPendingEngagementCheckpointStore inner, PendingEngagementCheckpointCache cache, int hits, int misses) : IPendingEngagementCheckpointStore
{
    public MemoryPendingEngagementCheckpointStore Inner { get; set; } = inner;
    public PendingEngagementCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingEngagementCheckpoint? Find(string code)
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

    public void Save(PendingEngagementCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingEngagementCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending engagement checkpoints. Empty fields match all.</summary>
public sealed record PendingEngagementCheckpointQuery(string Unit, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(PendingEngagementCheckpoint item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingEngagementCheckpoint> Apply(List<PendingEngagementCheckpoint> items)
    {
        var matched = Collections.Filter(items, (PendingEngagementCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending engagement checkpoint store.</summary>
public static class PendingEngagementCheckpointStore
{
    public static IPendingEngagementCheckpointStore NewMemoryPendingEngagementCheckpointStore()
    {
        return new MemoryPendingEngagementCheckpointStore(
            new Dictionary<string, PendingEngagementCheckpoint>(),
            new List<string>());
    }

    public static IPendingEngagementCheckpointStore NewCachedPendingEngagementCheckpointStore(MemoryPendingEngagementCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedPendingEngagementCheckpointStore(
            inner,
            PendingEngagementCheckpointCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PendingEngagementCheckpoint> FindPendingEngagementCheckpoints(IPendingEngagementCheckpointStore store, PendingEngagementCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
