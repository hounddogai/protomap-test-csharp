namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation checkpoints by code.</summary>
public interface IAttestationCheckpointStore
{
    AttestationCheckpoint? Find(string code);
    void Save(AttestationCheckpoint item);
    bool Remove(string code);
    List<AttestationCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationCheckpointStore(Dictionary<string, AttestationCheckpoint> items, List<string> order) : IAttestationCheckpointStore
{
    public Dictionary<string, AttestationCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationCheckpoint item)
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

    public List<AttestationCheckpoint> List()
    {
        var outValue = new List<AttestationCheckpoint>();
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
public sealed class CachedAttestationCheckpointStore(MemoryAttestationCheckpointStore inner, AttestationCheckpointCache cache, int hits, int misses) : IAttestationCheckpointStore
{
    public MemoryAttestationCheckpointStore Inner { get; set; } = inner;
    public AttestationCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationCheckpoint? Find(string code)
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

    public void Save(AttestationCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation checkpoints. Empty fields match all.</summary>
public sealed record AttestationCheckpointQuery(string Label, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationCheckpoint item)
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

    public List<AttestationCheckpoint> Apply(List<AttestationCheckpoint> items)
    {
        var matched = Collections.Filter(items, (AttestationCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation checkpoint store.</summary>
public static class AttestationCheckpointStore
{
    public static IAttestationCheckpointStore NewMemoryAttestationCheckpointStore()
    {
        return new MemoryAttestationCheckpointStore(
            new Dictionary<string, AttestationCheckpoint>(),
            new List<string>());
    }

    public static IAttestationCheckpointStore NewCachedAttestationCheckpointStore(MemoryAttestationCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationCheckpointStore(inner, AttestationCheckpointCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AttestationCheckpoint> FindAttestationCheckpoints(IAttestationCheckpointStore store, AttestationCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
