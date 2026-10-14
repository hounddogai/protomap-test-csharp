namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit checkpoints by code.</summary>
public interface IAuditCheckpointStore
{
    AuditCheckpoint? Find(string code);
    void Save(AuditCheckpoint item);
    bool Remove(string code);
    List<AuditCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditCheckpointStore(Dictionary<string, AuditCheckpoint> items, List<string> order) : IAuditCheckpointStore
{
    public Dictionary<string, AuditCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditCheckpoint item)
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

    public List<AuditCheckpoint> List()
    {
        var outValue = new List<AuditCheckpoint>();
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
public sealed class CachedAuditCheckpointStore(MemoryAuditCheckpointStore inner, AuditCheckpointCache cache, int hits, int misses) : IAuditCheckpointStore
{
    public MemoryAuditCheckpointStore Inner { get; set; } = inner;
    public AuditCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditCheckpoint? Find(string code)
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

    public void Save(AuditCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit checkpoints. Empty fields match all.</summary>
public sealed record AuditCheckpointQuery(string Country, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(AuditCheckpoint item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<AuditCheckpoint> Apply(List<AuditCheckpoint> items)
    {
        var matched = Collections.Filter(items, (AuditCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit checkpoint store.</summary>
public static class AuditCheckpointStore
{
    public static IAuditCheckpointStore NewMemoryAuditCheckpointStore()
    {
        return new MemoryAuditCheckpointStore(new Dictionary<string, AuditCheckpoint>(), new List<string>());
    }

    public static IAuditCheckpointStore NewCachedAuditCheckpointStore(MemoryAuditCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedAuditCheckpointStore(inner, AuditCheckpointCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AuditCheckpoint> FindAuditCheckpoints(IAuditCheckpointStore store, AuditCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
