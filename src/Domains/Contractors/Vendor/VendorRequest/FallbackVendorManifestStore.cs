namespace Acme.Workforce.Contractors.Vendor.VendorRequest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback vendor manifests by code.</summary>
public interface IFallbackVendorManifestStore
{
    FallbackVendorManifest? Find(string code);
    void Save(FallbackVendorManifest item);
    bool Remove(string code);
    List<FallbackVendorManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackVendorManifestStore(Dictionary<string, FallbackVendorManifest> items, List<string> order) : IFallbackVendorManifestStore
{
    public Dictionary<string, FallbackVendorManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackVendorManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackVendorManifest item)
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

    public List<FallbackVendorManifest> List()
    {
        var outValue = new List<FallbackVendorManifest>();
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
public sealed class CachedFallbackVendorManifestStore(MemoryFallbackVendorManifestStore inner, FallbackVendorManifestCache cache, int hits, int misses) : IFallbackVendorManifestStore
{
    public MemoryFallbackVendorManifestStore Inner { get; set; } = inner;
    public FallbackVendorManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackVendorManifest? Find(string code)
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

    public void Save(FallbackVendorManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackVendorManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback vendor manifests. Empty fields match all.</summary>
public sealed record FallbackVendorManifestQuery(string Label, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackVendorManifest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackVendorManifest> Apply(List<FallbackVendorManifest> items)
    {
        var matched = Collections.Filter(items, (FallbackVendorManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback vendor manifest store.</summary>
public static class FallbackVendorManifestStore
{
    public static IFallbackVendorManifestStore NewMemoryFallbackVendorManifestStore()
    {
        return new MemoryFallbackVendorManifestStore(
            new Dictionary<string, FallbackVendorManifest>(),
            new List<string>());
    }

    public static IFallbackVendorManifestStore NewCachedFallbackVendorManifestStore(MemoryFallbackVendorManifestStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackVendorManifestStore(
            inner,
            FallbackVendorManifestCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackVendorManifest> FindFallbackVendorManifests(IFallbackVendorManifestStore store, FallbackVendorManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
