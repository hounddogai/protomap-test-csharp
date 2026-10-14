namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy desk manifests by code.</summary>
public interface ILegacyDeskManifestStore
{
    LegacyDeskManifest? Find(string code);
    void Save(LegacyDeskManifest item);
    bool Remove(string code);
    List<LegacyDeskManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyDeskManifestStore(Dictionary<string, LegacyDeskManifest> items, List<string> order) : ILegacyDeskManifestStore
{
    public Dictionary<string, LegacyDeskManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyDeskManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyDeskManifest item)
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

    public List<LegacyDeskManifest> List()
    {
        var outValue = new List<LegacyDeskManifest>();
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
public sealed class CachedLegacyDeskManifestStore(MemoryLegacyDeskManifestStore inner, LegacyDeskManifestCache cache, int hits, int misses) : ILegacyDeskManifestStore
{
    public MemoryLegacyDeskManifestStore Inner { get; set; } = inner;
    public LegacyDeskManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyDeskManifest? Find(string code)
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

    public void Save(LegacyDeskManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyDeskManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy desk manifests. Empty fields match all.</summary>
public sealed record LegacyDeskManifestQuery(string Segment, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyDeskManifest item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<LegacyDeskManifest> Apply(List<LegacyDeskManifest> items)
    {
        var matched = Collections.Filter(items, (LegacyDeskManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy desk manifest store.</summary>
public static class LegacyDeskManifestStore
{
    public static ILegacyDeskManifestStore NewMemoryLegacyDeskManifestStore()
    {
        return new MemoryLegacyDeskManifestStore(new Dictionary<string, LegacyDeskManifest>(), new List<string>());
    }

    public static ILegacyDeskManifestStore NewCachedLegacyDeskManifestStore(MemoryLegacyDeskManifestStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyDeskManifestStore(inner, LegacyDeskManifestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LegacyDeskManifest> FindLegacyDeskManifests(ILegacyDeskManifestStore store, LegacyDeskManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
