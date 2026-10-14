namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy manifests by code.</summary>
public interface IOccupancyManifestStore
{
    OccupancyManifest? Find(string code);
    void Save(OccupancyManifest item);
    bool Remove(string code);
    List<OccupancyManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyManifestStore(Dictionary<string, OccupancyManifest> items, List<string> order) : IOccupancyManifestStore
{
    public Dictionary<string, OccupancyManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyManifest item)
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

    public List<OccupancyManifest> List()
    {
        var outValue = new List<OccupancyManifest>();
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
public sealed class CachedOccupancyManifestStore(MemoryOccupancyManifestStore inner, OccupancyManifestCache cache, int hits, int misses) : IOccupancyManifestStore
{
    public MemoryOccupancyManifestStore Inner { get; set; } = inner;
    public OccupancyManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyManifest? Find(string code)
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

    public void Save(OccupancyManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy manifests. Empty fields match all.</summary>
public sealed record OccupancyManifestQuery(string Category, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyManifest item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyManifest> Apply(List<OccupancyManifest> items)
    {
        var matched = Collections.Filter(items, (OccupancyManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy manifest store.</summary>
public static class OccupancyManifestStore
{
    public static IOccupancyManifestStore NewMemoryOccupancyManifestStore()
    {
        return new MemoryOccupancyManifestStore(new Dictionary<string, OccupancyManifest>(), new List<string>());
    }

    public static IOccupancyManifestStore NewCachedOccupancyManifestStore(MemoryOccupancyManifestStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyManifestStore(inner, OccupancyManifestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<OccupancyManifest> FindOccupancyManifests(IOccupancyManifestStore store, OccupancyManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
