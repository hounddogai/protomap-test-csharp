namespace Acme.Workforce.Compliance.Audit.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit quotas by code.</summary>
public interface IAuditQuotaStore
{
    AuditQuota? Find(string code);
    void Save(AuditQuota item);
    bool Remove(string code);
    List<AuditQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditQuotaStore(Dictionary<string, AuditQuota> items, List<string> order) : IAuditQuotaStore
{
    public Dictionary<string, AuditQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditQuota item)
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

    public List<AuditQuota> List()
    {
        var outValue = new List<AuditQuota>();
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
public sealed class CachedAuditQuotaStore(MemoryAuditQuotaStore inner, AuditQuotaCache cache, int hits, int misses) : IAuditQuotaStore
{
    public MemoryAuditQuotaStore Inner { get; set; } = inner;
    public AuditQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditQuota? Find(string code)
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

    public void Save(AuditQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit quotas. Empty fields match all.</summary>
public sealed record AuditQuotaQuery(string Category, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(AuditQuota item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditQuota> Apply(List<AuditQuota> items)
    {
        var matched = Collections.Filter(items, (AuditQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit quota store.</summary>
public static class AuditQuotaStore
{
    public static IAuditQuotaStore NewMemoryAuditQuotaStore()
    {
        return new MemoryAuditQuotaStore(new Dictionary<string, AuditQuota>(), new List<string>());
    }

    public static IAuditQuotaStore NewCachedAuditQuotaStore(MemoryAuditQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedAuditQuotaStore(inner, AuditQuotaCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AuditQuota> FindAuditQuotas(IAuditQuotaStore store, AuditQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
