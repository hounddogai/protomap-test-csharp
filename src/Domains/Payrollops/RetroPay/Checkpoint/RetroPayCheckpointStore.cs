namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay checkpoints by code.</summary>
public interface IRetroPayCheckpointStore
{
    RetroPayCheckpoint? Find(string code);
    void Save(RetroPayCheckpoint item);
    bool Remove(string code);
    List<RetroPayCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayCheckpointStore(Dictionary<string, RetroPayCheckpoint> items, List<string> order) : IRetroPayCheckpointStore
{
    public Dictionary<string, RetroPayCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayCheckpoint item)
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

    public List<RetroPayCheckpoint> List()
    {
        var outValue = new List<RetroPayCheckpoint>();
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
public sealed class CachedRetroPayCheckpointStore(MemoryRetroPayCheckpointStore inner, RetroPayCheckpointCache cache, int hits, int misses) : IRetroPayCheckpointStore
{
    public MemoryRetroPayCheckpointStore Inner { get; set; } = inner;
    public RetroPayCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayCheckpoint? Find(string code)
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

    public void Save(RetroPayCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay checkpoints. Empty fields match all.</summary>
public sealed record RetroPayCheckpointQuery(string Department, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayCheckpoint item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<RetroPayCheckpoint> Apply(List<RetroPayCheckpoint> items)
    {
        var matched = Collections.Filter(items, (RetroPayCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay checkpoint store.</summary>
public static class RetroPayCheckpointStore
{
    public static IRetroPayCheckpointStore NewMemoryRetroPayCheckpointStore()
    {
        return new MemoryRetroPayCheckpointStore(new Dictionary<string, RetroPayCheckpoint>(), new List<string>());
    }

    public static IRetroPayCheckpointStore NewCachedRetroPayCheckpointStore(MemoryRetroPayCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayCheckpointStore(inner, RetroPayCheckpointCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RetroPayCheckpoint> FindRetroPayCheckpoints(IRetroPayCheckpointStore store, RetroPayCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
