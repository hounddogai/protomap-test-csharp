namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary occupancy manifests by code.</summary>
public interface IPrimaryOccupancyManifestStore
{
    PrimaryOccupancyManifest? Find(string code);
    void Save(PrimaryOccupancyManifest item);
    bool Remove(string code);
    List<PrimaryOccupancyManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryOccupancyManifestStore(Dictionary<string, PrimaryOccupancyManifest> items, List<string> order) : IPrimaryOccupancyManifestStore
{
    public Dictionary<string, PrimaryOccupancyManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryOccupancyManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryOccupancyManifest item)
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

    public List<PrimaryOccupancyManifest> List()
    {
        var outValue = new List<PrimaryOccupancyManifest>();
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
public sealed class CachedPrimaryOccupancyManifestStore(MemoryPrimaryOccupancyManifestStore inner, PrimaryOccupancyManifestCache cache, int hits, int misses) : IPrimaryOccupancyManifestStore
{
    public MemoryPrimaryOccupancyManifestStore Inner { get; set; } = inner;
    public PrimaryOccupancyManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryOccupancyManifest? Find(string code)
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

    public void Save(PrimaryOccupancyManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryOccupancyManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary occupancy manifests. Empty fields match all.</summary>
public sealed record PrimaryOccupancyManifestQuery(string Source, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryOccupancyManifest item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryOccupancyManifest> Apply(List<PrimaryOccupancyManifest> items)
    {
        var matched = Collections.Filter(items, (PrimaryOccupancyManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary occupancy manifest store.</summary>
public static class PrimaryOccupancyManifestStore
{
    public static IPrimaryOccupancyManifestStore NewMemoryPrimaryOccupancyManifestStore()
    {
        return new MemoryPrimaryOccupancyManifestStore(
            new Dictionary<string, PrimaryOccupancyManifest>(),
            new List<string>());
    }

    public static IPrimaryOccupancyManifestStore NewCachedPrimaryOccupancyManifestStore(MemoryPrimaryOccupancyManifestStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryOccupancyManifestStore(
            inner,
            PrimaryOccupancyManifestCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryOccupancyManifest> FindPrimaryOccupancyManifests(IPrimaryOccupancyManifestStore store, PrimaryOccupancyManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
