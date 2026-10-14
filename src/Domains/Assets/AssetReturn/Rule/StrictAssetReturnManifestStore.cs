namespace Acme.Workforce.Assets.AssetReturn.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict asset return manifests by code.</summary>
public interface IStrictAssetReturnManifestStore
{
    StrictAssetReturnManifest? Find(string code);
    void Save(StrictAssetReturnManifest item);
    bool Remove(string code);
    List<StrictAssetReturnManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictAssetReturnManifestStore(Dictionary<string, StrictAssetReturnManifest> items, List<string> order) : IStrictAssetReturnManifestStore
{
    public Dictionary<string, StrictAssetReturnManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictAssetReturnManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictAssetReturnManifest item)
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

    public List<StrictAssetReturnManifest> List()
    {
        var outValue = new List<StrictAssetReturnManifest>();
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
public sealed class CachedStrictAssetReturnManifestStore(MemoryStrictAssetReturnManifestStore inner, StrictAssetReturnManifestCache cache, int hits, int misses) : IStrictAssetReturnManifestStore
{
    public MemoryStrictAssetReturnManifestStore Inner { get; set; } = inner;
    public StrictAssetReturnManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictAssetReturnManifest? Find(string code)
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

    public void Save(StrictAssetReturnManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictAssetReturnManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict asset return manifests. Empty fields match all.</summary>
public sealed record StrictAssetReturnManifestQuery(string Name, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(StrictAssetReturnManifest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictAssetReturnManifest> Apply(List<StrictAssetReturnManifest> items)
    {
        var matched = Collections.Filter(items, (StrictAssetReturnManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict asset return manifest store.</summary>
public static class StrictAssetReturnManifestStore
{
    public static IStrictAssetReturnManifestStore NewMemoryStrictAssetReturnManifestStore()
    {
        return new MemoryStrictAssetReturnManifestStore(
            new Dictionary<string, StrictAssetReturnManifest>(),
            new List<string>());
    }

    public static IStrictAssetReturnManifestStore NewCachedStrictAssetReturnManifestStore(MemoryStrictAssetReturnManifestStore inner, int ttl, IClock clock)
    {
        return new CachedStrictAssetReturnManifestStore(
            inner,
            StrictAssetReturnManifestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StrictAssetReturnManifest> FindStrictAssetReturnManifests(IStrictAssetReturnManifestStore store, StrictAssetReturnManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
