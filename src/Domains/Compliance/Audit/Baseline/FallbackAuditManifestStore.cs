namespace Acme.Workforce.Compliance.Audit.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback audit manifests by code.</summary>
public interface IFallbackAuditManifestStore
{
    FallbackAuditManifest? Find(string code);
    void Save(FallbackAuditManifest item);
    bool Remove(string code);
    List<FallbackAuditManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackAuditManifestStore(Dictionary<string, FallbackAuditManifest> items, List<string> order) : IFallbackAuditManifestStore
{
    public Dictionary<string, FallbackAuditManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackAuditManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackAuditManifest item)
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

    public List<FallbackAuditManifest> List()
    {
        var outValue = new List<FallbackAuditManifest>();
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
public sealed class CachedFallbackAuditManifestStore(MemoryFallbackAuditManifestStore inner, FallbackAuditManifestCache cache, int hits, int misses) : IFallbackAuditManifestStore
{
    public MemoryFallbackAuditManifestStore Inner { get; set; } = inner;
    public FallbackAuditManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackAuditManifest? Find(string code)
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

    public void Save(FallbackAuditManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackAuditManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback audit manifests. Empty fields match all.</summary>
public sealed record FallbackAuditManifestQuery(string Locale, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackAuditManifest item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackAuditManifest> Apply(List<FallbackAuditManifest> items)
    {
        var matched = Collections.Filter(items, (FallbackAuditManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback audit manifest store.</summary>
public static class FallbackAuditManifestStore
{
    public static IFallbackAuditManifestStore NewMemoryFallbackAuditManifestStore()
    {
        return new MemoryFallbackAuditManifestStore(
            new Dictionary<string, FallbackAuditManifest>(),
            new List<string>());
    }

    public static IFallbackAuditManifestStore NewCachedFallbackAuditManifestStore(MemoryFallbackAuditManifestStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackAuditManifestStore(inner, FallbackAuditManifestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<FallbackAuditManifest> FindFallbackAuditManifests(IFallbackAuditManifestStore store, FallbackAuditManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
