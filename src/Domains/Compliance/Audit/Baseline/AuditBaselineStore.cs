namespace Acme.Workforce.Compliance.Audit.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit baselines by code.</summary>
public interface IAuditBaselineStore
{
    AuditBaseline? Find(string code);
    void Save(AuditBaseline item);
    bool Remove(string code);
    List<AuditBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditBaselineStore(Dictionary<string, AuditBaseline> items, List<string> order) : IAuditBaselineStore
{
    public Dictionary<string, AuditBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditBaseline item)
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

    public List<AuditBaseline> List()
    {
        var outValue = new List<AuditBaseline>();
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
public sealed class CachedAuditBaselineStore(MemoryAuditBaselineStore inner, AuditBaselineCache cache, int hits, int misses) : IAuditBaselineStore
{
    public MemoryAuditBaselineStore Inner { get; set; } = inner;
    public AuditBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditBaseline? Find(string code)
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

    public void Save(AuditBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit baselines. Empty fields match all.</summary>
public sealed record AuditBaselineQuery(string Region, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(AuditBaseline item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditBaseline> Apply(List<AuditBaseline> items)
    {
        var matched = Collections.Filter(items, (AuditBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit baseline store.</summary>
public static class AuditBaselineStore
{
    public static IAuditBaselineStore NewMemoryAuditBaselineStore()
    {
        return new MemoryAuditBaselineStore(new Dictionary<string, AuditBaseline>(), new List<string>());
    }

    public static IAuditBaselineStore NewCachedAuditBaselineStore(MemoryAuditBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedAuditBaselineStore(inner, AuditBaselineCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AuditBaseline> FindAuditBaselines(IAuditBaselineStore store, AuditBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
