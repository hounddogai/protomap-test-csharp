namespace Acme.Workforce.Talent.Pool.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool manifests by code.</summary>
public interface IPoolManifestStore
{
    PoolManifest? Find(string code);
    void Save(PoolManifest item);
    bool Remove(string code);
    List<PoolManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolManifestStore(Dictionary<string, PoolManifest> items, List<string> order) : IPoolManifestStore
{
    public Dictionary<string, PoolManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolManifest item)
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

    public List<PoolManifest> List()
    {
        var outValue = new List<PoolManifest>();
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
public sealed class CachedPoolManifestStore(MemoryPoolManifestStore inner, PoolManifestCache cache, int hits, int misses) : IPoolManifestStore
{
    public MemoryPoolManifestStore Inner { get; set; } = inner;
    public PoolManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolManifest? Find(string code)
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

    public void Save(PoolManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool manifests. Empty fields match all.</summary>
public sealed record PoolManifestQuery(string Region, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(PoolManifest item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolManifest> Apply(List<PoolManifest> items)
    {
        var matched = Collections.Filter(items, (PoolManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool manifest store.</summary>
public static class PoolManifestStore
{
    public static IPoolManifestStore NewMemoryPoolManifestStore()
    {
        return new MemoryPoolManifestStore(new Dictionary<string, PoolManifest>(), new List<string>());
    }

    public static IPoolManifestStore NewCachedPoolManifestStore(MemoryPoolManifestStore inner, int ttl, IClock clock)
    {
        return new CachedPoolManifestStore(inner, PoolManifestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PoolManifest> FindPoolManifests(IPoolManifestStore store, PoolManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
