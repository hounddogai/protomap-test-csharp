namespace Acme.Workforce.Orgdesign.Position.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling position manifests by code.</summary>
public interface IRollingPositionManifestStore
{
    RollingPositionManifest? Find(string code);
    void Save(RollingPositionManifest item);
    bool Remove(string code);
    List<RollingPositionManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingPositionManifestStore(Dictionary<string, RollingPositionManifest> items, List<string> order) : IRollingPositionManifestStore
{
    public Dictionary<string, RollingPositionManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingPositionManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingPositionManifest item)
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

    public List<RollingPositionManifest> List()
    {
        var outValue = new List<RollingPositionManifest>();
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
public sealed class CachedRollingPositionManifestStore(MemoryRollingPositionManifestStore inner, RollingPositionManifestCache cache, int hits, int misses) : IRollingPositionManifestStore
{
    public MemoryRollingPositionManifestStore Inner { get; set; } = inner;
    public RollingPositionManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingPositionManifest? Find(string code)
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

    public void Save(RollingPositionManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingPositionManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling position manifests. Empty fields match all.</summary>
public sealed record RollingPositionManifestQuery(string Segment, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(RollingPositionManifest item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingPositionManifest> Apply(List<RollingPositionManifest> items)
    {
        var matched = Collections.Filter(items, (RollingPositionManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling position manifest store.</summary>
public static class RollingPositionManifestStore
{
    public static IRollingPositionManifestStore NewMemoryRollingPositionManifestStore()
    {
        return new MemoryRollingPositionManifestStore(
            new Dictionary<string, RollingPositionManifest>(),
            new List<string>());
    }

    public static IRollingPositionManifestStore NewCachedRollingPositionManifestStore(MemoryRollingPositionManifestStore inner, int ttl, IClock clock)
    {
        return new CachedRollingPositionManifestStore(
            inner,
            RollingPositionManifestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RollingPositionManifest> FindRollingPositionManifests(IRollingPositionManifestStore store, RollingPositionManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
