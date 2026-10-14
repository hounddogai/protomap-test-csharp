namespace Acme.Workforce.Compliance.Audit.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit bundles by code.</summary>
public interface IAuditBundleStore
{
    AuditBundle? Find(string code);
    void Save(AuditBundle item);
    bool Remove(string code);
    List<AuditBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditBundleStore(Dictionary<string, AuditBundle> items, List<string> order) : IAuditBundleStore
{
    public Dictionary<string, AuditBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditBundle item)
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

    public List<AuditBundle> List()
    {
        var outValue = new List<AuditBundle>();
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
public sealed class CachedAuditBundleStore(MemoryAuditBundleStore inner, AuditBundleCache cache, int hits, int misses) : IAuditBundleStore
{
    public MemoryAuditBundleStore Inner { get; set; } = inner;
    public AuditBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditBundle? Find(string code)
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

    public void Save(AuditBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit bundles. Empty fields match all.</summary>
public sealed record AuditBundleQuery(string Region, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(AuditBundle item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<AuditBundle> Apply(List<AuditBundle> items)
    {
        var matched = Collections.Filter(items, (AuditBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit bundle store.</summary>
public static class AuditBundleStore
{
    public static IAuditBundleStore NewMemoryAuditBundleStore()
    {
        return new MemoryAuditBundleStore(new Dictionary<string, AuditBundle>(), new List<string>());
    }

    public static IAuditBundleStore NewCachedAuditBundleStore(MemoryAuditBundleStore inner, int ttl, IClock clock)
    {
        return new CachedAuditBundleStore(inner, AuditBundleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AuditBundle> FindAuditBundles(IAuditBundleStore store, AuditBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
