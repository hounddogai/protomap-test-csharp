namespace Acme.Workforce.Assets.Warranty.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict warranty manifests by code.</summary>
public interface IStrictWarrantyManifestStore
{
    StrictWarrantyManifest? Find(string code);
    void Save(StrictWarrantyManifest item);
    bool Remove(string code);
    List<StrictWarrantyManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictWarrantyManifestStore(Dictionary<string, StrictWarrantyManifest> items, List<string> order) : IStrictWarrantyManifestStore
{
    public Dictionary<string, StrictWarrantyManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictWarrantyManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictWarrantyManifest item)
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

    public List<StrictWarrantyManifest> List()
    {
        var outValue = new List<StrictWarrantyManifest>();
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
public sealed class CachedStrictWarrantyManifestStore(MemoryStrictWarrantyManifestStore inner, StrictWarrantyManifestCache cache, int hits, int misses) : IStrictWarrantyManifestStore
{
    public MemoryStrictWarrantyManifestStore Inner { get; set; } = inner;
    public StrictWarrantyManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictWarrantyManifest? Find(string code)
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

    public void Save(StrictWarrantyManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictWarrantyManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict warranty manifests. Empty fields match all.</summary>
public sealed record StrictWarrantyManifestQuery(string Channel, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(StrictWarrantyManifest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictWarrantyManifest> Apply(List<StrictWarrantyManifest> items)
    {
        var matched = Collections.Filter(items, (StrictWarrantyManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict warranty manifest store.</summary>
public static class StrictWarrantyManifestStore
{
    public static IStrictWarrantyManifestStore NewMemoryStrictWarrantyManifestStore()
    {
        return new MemoryStrictWarrantyManifestStore(
            new Dictionary<string, StrictWarrantyManifest>(),
            new List<string>());
    }

    public static IStrictWarrantyManifestStore NewCachedStrictWarrantyManifestStore(MemoryStrictWarrantyManifestStore inner, int ttl, IClock clock)
    {
        return new CachedStrictWarrantyManifestStore(
            inner,
            StrictWarrantyManifestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StrictWarrantyManifest> FindStrictWarrantyManifests(IStrictWarrantyManifestStore store, StrictWarrantyManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
