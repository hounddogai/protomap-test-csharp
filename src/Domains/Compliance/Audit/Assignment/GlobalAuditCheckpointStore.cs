namespace Acme.Workforce.Compliance.Audit.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global audit checkpoints by code.</summary>
public interface IGlobalAuditCheckpointStore
{
    GlobalAuditCheckpoint? Find(string code);
    void Save(GlobalAuditCheckpoint item);
    bool Remove(string code);
    List<GlobalAuditCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalAuditCheckpointStore(Dictionary<string, GlobalAuditCheckpoint> items, List<string> order) : IGlobalAuditCheckpointStore
{
    public Dictionary<string, GlobalAuditCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalAuditCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalAuditCheckpoint item)
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

    public List<GlobalAuditCheckpoint> List()
    {
        var outValue = new List<GlobalAuditCheckpoint>();
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
public sealed class CachedGlobalAuditCheckpointStore(MemoryGlobalAuditCheckpointStore inner, GlobalAuditCheckpointCache cache, int hits, int misses) : IGlobalAuditCheckpointStore
{
    public MemoryGlobalAuditCheckpointStore Inner { get; set; } = inner;
    public GlobalAuditCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalAuditCheckpoint? Find(string code)
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

    public void Save(GlobalAuditCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalAuditCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global audit checkpoints. Empty fields match all.</summary>
public sealed record GlobalAuditCheckpointQuery(string Name, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalAuditCheckpoint item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<GlobalAuditCheckpoint> Apply(List<GlobalAuditCheckpoint> items)
    {
        var matched = Collections.Filter(items, (GlobalAuditCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global audit checkpoint store.</summary>
public static class GlobalAuditCheckpointStore
{
    public static IGlobalAuditCheckpointStore NewMemoryGlobalAuditCheckpointStore()
    {
        return new MemoryGlobalAuditCheckpointStore(
            new Dictionary<string, GlobalAuditCheckpoint>(),
            new List<string>());
    }

    public static IGlobalAuditCheckpointStore NewCachedGlobalAuditCheckpointStore(MemoryGlobalAuditCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalAuditCheckpointStore(inner, GlobalAuditCheckpointCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<GlobalAuditCheckpoint> FindGlobalAuditCheckpoints(IGlobalAuditCheckpointStore store, GlobalAuditCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
