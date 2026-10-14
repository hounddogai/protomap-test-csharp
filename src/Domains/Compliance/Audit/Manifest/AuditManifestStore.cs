namespace Acme.Workforce.Compliance.Audit.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit manifests by code.</summary>
public interface IAuditManifestStore
{
    AuditManifest? Find(string code);
    void Save(AuditManifest item);
    bool Remove(string code);
    List<AuditManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditManifestStore(Dictionary<string, AuditManifest> items, List<string> order) : IAuditManifestStore
{
    public Dictionary<string, AuditManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditManifest item)
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

    public List<AuditManifest> List()
    {
        var outValue = new List<AuditManifest>();
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
public sealed class CachedAuditManifestStore(MemoryAuditManifestStore inner, AuditManifestCache cache, int hits, int misses) : IAuditManifestStore
{
    public MemoryAuditManifestStore Inner { get; set; } = inner;
    public AuditManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditManifest? Find(string code)
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

    public void Save(AuditManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit manifests. Empty fields match all.</summary>
public sealed record AuditManifestQuery(string Department, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(AuditManifest item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditManifest> Apply(List<AuditManifest> items)
    {
        var matched = Collections.Filter(items, (AuditManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit manifest store.</summary>
public static class AuditManifestStore
{
    public static IAuditManifestStore NewMemoryAuditManifestStore()
    {
        return new MemoryAuditManifestStore(new Dictionary<string, AuditManifest>(), new List<string>());
    }

    public static IAuditManifestStore NewCachedAuditManifestStore(MemoryAuditManifestStore inner, int ttl, IClock clock)
    {
        return new CachedAuditManifestStore(inner, AuditManifestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AuditManifest> FindAuditManifests(IAuditManifestStore store, AuditManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
