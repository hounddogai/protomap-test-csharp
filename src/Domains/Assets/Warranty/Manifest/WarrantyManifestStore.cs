namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty manifests by code.</summary>
public interface IWarrantyManifestStore
{
    WarrantyManifest? Find(string code);
    void Save(WarrantyManifest item);
    bool Remove(string code);
    List<WarrantyManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyManifestStore(Dictionary<string, WarrantyManifest> items, List<string> order) : IWarrantyManifestStore
{
    public Dictionary<string, WarrantyManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyManifest item)
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

    public List<WarrantyManifest> List()
    {
        var outValue = new List<WarrantyManifest>();
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
public sealed class CachedWarrantyManifestStore(MemoryWarrantyManifestStore inner, WarrantyManifestCache cache, int hits, int misses) : IWarrantyManifestStore
{
    public MemoryWarrantyManifestStore Inner { get; set; } = inner;
    public WarrantyManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyManifest? Find(string code)
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

    public void Save(WarrantyManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty manifests. Empty fields match all.</summary>
public sealed record WarrantyManifestQuery(string Country, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyManifest item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<WarrantyManifest> Apply(List<WarrantyManifest> items)
    {
        var matched = Collections.Filter(items, (WarrantyManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty manifest store.</summary>
public static class WarrantyManifestStore
{
    public static IWarrantyManifestStore NewMemoryWarrantyManifestStore()
    {
        return new MemoryWarrantyManifestStore(new Dictionary<string, WarrantyManifest>(), new List<string>());
    }

    public static IWarrantyManifestStore NewCachedWarrantyManifestStore(MemoryWarrantyManifestStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyManifestStore(inner, WarrantyManifestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<WarrantyManifest> FindWarrantyManifests(IWarrantyManifestStore store, WarrantyManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
