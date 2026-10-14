namespace Acme.Workforce.Talent.Pool.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool audits by code.</summary>
public interface IPoolAuditStore
{
    PoolAudit? Find(string code);
    void Save(PoolAudit item);
    bool Remove(string code);
    List<PoolAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolAuditStore(Dictionary<string, PoolAudit> items, List<string> order) : IPoolAuditStore
{
    public Dictionary<string, PoolAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolAudit item)
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

    public List<PoolAudit> List()
    {
        var outValue = new List<PoolAudit>();
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
public sealed class CachedPoolAuditStore(MemoryPoolAuditStore inner, PoolAuditCache cache, int hits, int misses) : IPoolAuditStore
{
    public MemoryPoolAuditStore Inner { get; set; } = inner;
    public PoolAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolAudit? Find(string code)
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

    public void Save(PoolAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool audits. Empty fields match all.</summary>
public sealed record PoolAuditQuery(string Segment, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(PoolAudit item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolAudit> Apply(List<PoolAudit> items)
    {
        var matched = Collections.Filter(items, (PoolAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool audit store.</summary>
public static class PoolAuditStore
{
    public static IPoolAuditStore NewMemoryPoolAuditStore()
    {
        return new MemoryPoolAuditStore(new Dictionary<string, PoolAudit>(), new List<string>());
    }

    public static IPoolAuditStore NewCachedPoolAuditStore(MemoryPoolAuditStore inner, int ttl, IClock clock)
    {
        return new CachedPoolAuditStore(inner, PoolAuditCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PoolAudit> FindPoolAudits(IPoolAuditStore store, PoolAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
