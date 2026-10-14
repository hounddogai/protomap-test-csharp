namespace Acme.Workforce.Compliance.Audit.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit policies by code.</summary>
public interface IAuditPolicyStore
{
    AuditPolicy? Find(string code);
    void Save(AuditPolicy item);
    bool Remove(string code);
    List<AuditPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditPolicyStore(Dictionary<string, AuditPolicy> items, List<string> order) : IAuditPolicyStore
{
    public Dictionary<string, AuditPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditPolicy item)
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

    public List<AuditPolicy> List()
    {
        var outValue = new List<AuditPolicy>();
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
public sealed class CachedAuditPolicyStore(MemoryAuditPolicyStore inner, AuditPolicyCache cache, int hits, int misses) : IAuditPolicyStore
{
    public MemoryAuditPolicyStore Inner { get; set; } = inner;
    public AuditPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditPolicy? Find(string code)
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

    public void Save(AuditPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit policies. Empty fields match all.</summary>
public sealed record AuditPolicyQuery(string Segment, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(AuditPolicy item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditPolicy> Apply(List<AuditPolicy> items)
    {
        var matched = Collections.Filter(items, (AuditPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit policy store.</summary>
public static class AuditPolicyStore
{
    public static IAuditPolicyStore NewMemoryAuditPolicyStore()
    {
        return new MemoryAuditPolicyStore(new Dictionary<string, AuditPolicy>(), new List<string>());
    }

    public static IAuditPolicyStore NewCachedAuditPolicyStore(MemoryAuditPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedAuditPolicyStore(inner, AuditPolicyCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AuditPolicy> FindAuditPolicies(IAuditPolicyStore store, AuditPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
