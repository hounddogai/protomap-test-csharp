namespace Acme.Workforce.Compliance.Audit.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default audit checkpoints by code.</summary>
public interface IDefaultAuditCheckpointStore
{
    DefaultAuditCheckpoint? Find(string code);
    void Save(DefaultAuditCheckpoint item);
    bool Remove(string code);
    List<DefaultAuditCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultAuditCheckpointStore(Dictionary<string, DefaultAuditCheckpoint> items, List<string> order) : IDefaultAuditCheckpointStore
{
    public Dictionary<string, DefaultAuditCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultAuditCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultAuditCheckpoint item)
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

    public List<DefaultAuditCheckpoint> List()
    {
        var outValue = new List<DefaultAuditCheckpoint>();
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
public sealed class CachedDefaultAuditCheckpointStore(MemoryDefaultAuditCheckpointStore inner, DefaultAuditCheckpointCache cache, int hits, int misses) : IDefaultAuditCheckpointStore
{
    public MemoryDefaultAuditCheckpointStore Inner { get; set; } = inner;
    public DefaultAuditCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultAuditCheckpoint? Find(string code)
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

    public void Save(DefaultAuditCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultAuditCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default audit checkpoints. Empty fields match all.</summary>
public sealed record DefaultAuditCheckpointQuery(string Category, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultAuditCheckpoint item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultAuditCheckpoint> Apply(List<DefaultAuditCheckpoint> items)
    {
        var matched = Collections.Filter(items, (DefaultAuditCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default audit checkpoint store.</summary>
public static class DefaultAuditCheckpointStore
{
    public static IDefaultAuditCheckpointStore NewMemoryDefaultAuditCheckpointStore()
    {
        return new MemoryDefaultAuditCheckpointStore(
            new Dictionary<string, DefaultAuditCheckpoint>(),
            new List<string>());
    }

    public static IDefaultAuditCheckpointStore NewCachedDefaultAuditCheckpointStore(MemoryDefaultAuditCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultAuditCheckpointStore(
            inner,
            DefaultAuditCheckpointCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<DefaultAuditCheckpoint> FindDefaultAuditCheckpoints(IDefaultAuditCheckpointStore store, DefaultAuditCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
