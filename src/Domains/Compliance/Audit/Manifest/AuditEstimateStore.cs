namespace Acme.Workforce.Compliance.Audit.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit estimates by code.</summary>
public interface IAuditEstimateStore
{
    AuditEstimate? Find(string code);
    void Save(AuditEstimate item);
    bool Remove(string code);
    List<AuditEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditEstimateStore(Dictionary<string, AuditEstimate> items, List<string> order) : IAuditEstimateStore
{
    public Dictionary<string, AuditEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditEstimate item)
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

    public List<AuditEstimate> List()
    {
        var outValue = new List<AuditEstimate>();
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
public sealed class CachedAuditEstimateStore(MemoryAuditEstimateStore inner, AuditEstimateCache cache, int hits, int misses) : IAuditEstimateStore
{
    public MemoryAuditEstimateStore Inner { get; set; } = inner;
    public AuditEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditEstimate? Find(string code)
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

    public void Save(AuditEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit estimates. Empty fields match all.</summary>
public sealed record AuditEstimateQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AuditEstimate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditEstimate> Apply(List<AuditEstimate> items)
    {
        var matched = Collections.Filter(items, (AuditEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit estimate store.</summary>
public static class AuditEstimateStore
{
    public static IAuditEstimateStore NewMemoryAuditEstimateStore()
    {
        return new MemoryAuditEstimateStore(new Dictionary<string, AuditEstimate>(), new List<string>());
    }

    public static IAuditEstimateStore NewCachedAuditEstimateStore(MemoryAuditEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedAuditEstimateStore(inner, AuditEstimateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AuditEstimate> FindAuditEstimates(IAuditEstimateStore store, AuditEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
