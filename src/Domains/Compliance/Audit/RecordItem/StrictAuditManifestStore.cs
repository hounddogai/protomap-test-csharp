namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict audit manifests by code.</summary>
public interface IStrictAuditManifestStore
{
    StrictAuditManifest? Find(string code);
    void Save(StrictAuditManifest item);
    bool Remove(string code);
    List<StrictAuditManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictAuditManifestStore(Dictionary<string, StrictAuditManifest> items, List<string> order) : IStrictAuditManifestStore
{
    public Dictionary<string, StrictAuditManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictAuditManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictAuditManifest item)
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

    public List<StrictAuditManifest> List()
    {
        var outValue = new List<StrictAuditManifest>();
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
public sealed class CachedStrictAuditManifestStore(MemoryStrictAuditManifestStore inner, StrictAuditManifestCache cache, int hits, int misses) : IStrictAuditManifestStore
{
    public MemoryStrictAuditManifestStore Inner { get; set; } = inner;
    public StrictAuditManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictAuditManifest? Find(string code)
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

    public void Save(StrictAuditManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictAuditManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict audit manifests. Empty fields match all.</summary>
public sealed record StrictAuditManifestQuery(string Title, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(StrictAuditManifest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictAuditManifest> Apply(List<StrictAuditManifest> items)
    {
        var matched = Collections.Filter(items, (StrictAuditManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict audit manifest store.</summary>
public static class StrictAuditManifestStore
{
    public static IStrictAuditManifestStore NewMemoryStrictAuditManifestStore()
    {
        return new MemoryStrictAuditManifestStore(new Dictionary<string, StrictAuditManifest>(), new List<string>());
    }

    public static IStrictAuditManifestStore NewCachedStrictAuditManifestStore(MemoryStrictAuditManifestStore inner, int ttl, IClock clock)
    {
        return new CachedStrictAuditManifestStore(inner, StrictAuditManifestCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StrictAuditManifest> FindStrictAuditManifests(IStrictAuditManifestStore store, StrictAuditManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
