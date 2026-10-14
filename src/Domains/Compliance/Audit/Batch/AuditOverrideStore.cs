namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit overrides by code.</summary>
public interface IAuditOverrideStore
{
    AuditOverride? Find(string code);
    void Save(AuditOverride item);
    bool Remove(string code);
    List<AuditOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditOverrideStore(Dictionary<string, AuditOverride> items, List<string> order) : IAuditOverrideStore
{
    public Dictionary<string, AuditOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditOverride item)
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

    public List<AuditOverride> List()
    {
        var outValue = new List<AuditOverride>();
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
public sealed class CachedAuditOverrideStore(MemoryAuditOverrideStore inner, AuditOverrideCache cache, int hits, int misses) : IAuditOverrideStore
{
    public MemoryAuditOverrideStore Inner { get; set; } = inner;
    public AuditOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditOverride? Find(string code)
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

    public void Save(AuditOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit overrides. Empty fields match all.</summary>
public sealed record AuditOverrideQuery(string Region, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(AuditOverride item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<AuditOverride> Apply(List<AuditOverride> items)
    {
        var matched = Collections.Filter(items, (AuditOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit override store.</summary>
public static class AuditOverrideStore
{
    public static IAuditOverrideStore NewMemoryAuditOverrideStore()
    {
        return new MemoryAuditOverrideStore(new Dictionary<string, AuditOverride>(), new List<string>());
    }

    public static IAuditOverrideStore NewCachedAuditOverrideStore(MemoryAuditOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedAuditOverrideStore(inner, AuditOverrideCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AuditOverride> FindAuditOverrides(IAuditOverrideStore store, AuditOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
