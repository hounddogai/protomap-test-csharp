namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit claims by code.</summary>
public interface IAuditClaimStore
{
    AuditClaim? Find(string code);
    void Save(AuditClaim item);
    bool Remove(string code);
    List<AuditClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditClaimStore(Dictionary<string, AuditClaim> items, List<string> order) : IAuditClaimStore
{
    public Dictionary<string, AuditClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditClaim item)
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

    public List<AuditClaim> List()
    {
        var outValue = new List<AuditClaim>();
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
public sealed class CachedAuditClaimStore(MemoryAuditClaimStore inner, AuditClaimCache cache, int hits, int misses) : IAuditClaimStore
{
    public MemoryAuditClaimStore Inner { get; set; } = inner;
    public AuditClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditClaim? Find(string code)
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

    public void Save(AuditClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit claims. Empty fields match all.</summary>
public sealed record AuditClaimQuery(string Name, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(AuditClaim item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditClaim> Apply(List<AuditClaim> items)
    {
        var matched = Collections.Filter(items, (AuditClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit claim store.</summary>
public static class AuditClaimStore
{
    public static IAuditClaimStore NewMemoryAuditClaimStore()
    {
        return new MemoryAuditClaimStore(new Dictionary<string, AuditClaim>(), new List<string>());
    }

    public static IAuditClaimStore NewCachedAuditClaimStore(MemoryAuditClaimStore inner, int ttl, IClock clock)
    {
        return new CachedAuditClaimStore(inner, AuditClaimCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AuditClaim> FindAuditClaims(IAuditClaimStore store, AuditClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
