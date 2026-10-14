namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit rollups by code.</summary>
public interface IAuditRollupStore
{
    AuditRollup? Find(string code);
    void Save(AuditRollup item);
    bool Remove(string code);
    List<AuditRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditRollupStore(Dictionary<string, AuditRollup> items, List<string> order) : IAuditRollupStore
{
    public Dictionary<string, AuditRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditRollup item)
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

    public List<AuditRollup> List()
    {
        var outValue = new List<AuditRollup>();
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
public sealed class CachedAuditRollupStore(MemoryAuditRollupStore inner, AuditRollupCache cache, int hits, int misses) : IAuditRollupStore
{
    public MemoryAuditRollupStore Inner { get; set; } = inner;
    public AuditRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditRollup? Find(string code)
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

    public void Save(AuditRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit rollups. Empty fields match all.</summary>
public sealed record AuditRollupQuery(string Category, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(AuditRollup item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditRollup> Apply(List<AuditRollup> items)
    {
        var matched = Collections.Filter(items, (AuditRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit rollup store.</summary>
public static class AuditRollupStore
{
    public static IAuditRollupStore NewMemoryAuditRollupStore()
    {
        return new MemoryAuditRollupStore(new Dictionary<string, AuditRollup>(), new List<string>());
    }

    public static IAuditRollupStore NewCachedAuditRollupStore(MemoryAuditRollupStore inner, int ttl, IClock clock)
    {
        return new CachedAuditRollupStore(inner, AuditRollupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AuditRollup> FindAuditRollups(IAuditRollupStore store, AuditRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
