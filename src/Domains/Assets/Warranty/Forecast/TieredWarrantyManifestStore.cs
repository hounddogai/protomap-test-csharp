namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered warranty manifests by code.</summary>
public interface ITieredWarrantyManifestStore
{
    TieredWarrantyManifest? Find(string code);
    void Save(TieredWarrantyManifest item);
    bool Remove(string code);
    List<TieredWarrantyManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredWarrantyManifestStore(Dictionary<string, TieredWarrantyManifest> items, List<string> order) : ITieredWarrantyManifestStore
{
    public Dictionary<string, TieredWarrantyManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredWarrantyManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredWarrantyManifest item)
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

    public List<TieredWarrantyManifest> List()
    {
        var outValue = new List<TieredWarrantyManifest>();
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
public sealed class CachedTieredWarrantyManifestStore(MemoryTieredWarrantyManifestStore inner, TieredWarrantyManifestCache cache, int hits, int misses) : ITieredWarrantyManifestStore
{
    public MemoryTieredWarrantyManifestStore Inner { get; set; } = inner;
    public TieredWarrantyManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredWarrantyManifest? Find(string code)
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

    public void Save(TieredWarrantyManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredWarrantyManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered warranty manifests. Empty fields match all.</summary>
public sealed record TieredWarrantyManifestQuery(string Region, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(TieredWarrantyManifest item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<TieredWarrantyManifest> Apply(List<TieredWarrantyManifest> items)
    {
        var matched = Collections.Filter(items, (TieredWarrantyManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered warranty manifest store.</summary>
public static class TieredWarrantyManifestStore
{
    public static ITieredWarrantyManifestStore NewMemoryTieredWarrantyManifestStore()
    {
        return new MemoryTieredWarrantyManifestStore(
            new Dictionary<string, TieredWarrantyManifest>(),
            new List<string>());
    }

    public static ITieredWarrantyManifestStore NewCachedTieredWarrantyManifestStore(MemoryTieredWarrantyManifestStore inner, int ttl, IClock clock)
    {
        return new CachedTieredWarrantyManifestStore(
            inner,
            TieredWarrantyManifestCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<TieredWarrantyManifest> FindTieredWarrantyManifests(ITieredWarrantyManifestStore store, TieredWarrantyManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
