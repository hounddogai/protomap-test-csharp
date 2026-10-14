namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global attestation checkpoints by code.</summary>
public interface IGlobalAttestationCheckpointStore
{
    GlobalAttestationCheckpoint? Find(string code);
    void Save(GlobalAttestationCheckpoint item);
    bool Remove(string code);
    List<GlobalAttestationCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalAttestationCheckpointStore(Dictionary<string, GlobalAttestationCheckpoint> items, List<string> order) : IGlobalAttestationCheckpointStore
{
    public Dictionary<string, GlobalAttestationCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalAttestationCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalAttestationCheckpoint item)
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

    public List<GlobalAttestationCheckpoint> List()
    {
        var outValue = new List<GlobalAttestationCheckpoint>();
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
public sealed class CachedGlobalAttestationCheckpointStore(MemoryGlobalAttestationCheckpointStore inner, GlobalAttestationCheckpointCache cache, int hits, int misses) : IGlobalAttestationCheckpointStore
{
    public MemoryGlobalAttestationCheckpointStore Inner { get; set; } = inner;
    public GlobalAttestationCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalAttestationCheckpoint? Find(string code)
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

    public void Save(GlobalAttestationCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalAttestationCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global attestation checkpoints. Empty fields match all.</summary>
public sealed record GlobalAttestationCheckpointQuery(string Label, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalAttestationCheckpoint item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<GlobalAttestationCheckpoint> Apply(List<GlobalAttestationCheckpoint> items)
    {
        var matched = Collections.Filter(items, (GlobalAttestationCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global attestation checkpoint store.</summary>
public static class GlobalAttestationCheckpointStore
{
    public static IGlobalAttestationCheckpointStore NewMemoryGlobalAttestationCheckpointStore()
    {
        return new MemoryGlobalAttestationCheckpointStore(
            new Dictionary<string, GlobalAttestationCheckpoint>(),
            new List<string>());
    }

    public static IGlobalAttestationCheckpointStore NewCachedGlobalAttestationCheckpointStore(MemoryGlobalAttestationCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalAttestationCheckpointStore(
            inner,
            GlobalAttestationCheckpointCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<GlobalAttestationCheckpoint> FindGlobalAttestationCheckpoints(IGlobalAttestationCheckpointStore store, GlobalAttestationCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
