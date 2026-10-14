namespace Acme.Workforce.Engagement.TownHall.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted town hall manifests by code.</summary>
public interface IWeightedTownHallManifestStore
{
    WeightedTownHallManifest? Find(string code);
    void Save(WeightedTownHallManifest item);
    bool Remove(string code);
    List<WeightedTownHallManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedTownHallManifestStore(Dictionary<string, WeightedTownHallManifest> items, List<string> order) : IWeightedTownHallManifestStore
{
    public Dictionary<string, WeightedTownHallManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedTownHallManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedTownHallManifest item)
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

    public List<WeightedTownHallManifest> List()
    {
        var outValue = new List<WeightedTownHallManifest>();
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
public sealed class CachedWeightedTownHallManifestStore(MemoryWeightedTownHallManifestStore inner, WeightedTownHallManifestCache cache, int hits, int misses) : IWeightedTownHallManifestStore
{
    public MemoryWeightedTownHallManifestStore Inner { get; set; } = inner;
    public WeightedTownHallManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedTownHallManifest? Find(string code)
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

    public void Save(WeightedTownHallManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedTownHallManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted town hall manifests. Empty fields match all.</summary>
public sealed record WeightedTownHallManifestQuery(string Name, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedTownHallManifest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<WeightedTownHallManifest> Apply(List<WeightedTownHallManifest> items)
    {
        var matched = Collections.Filter(items, (WeightedTownHallManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted town hall manifest store.</summary>
public static class WeightedTownHallManifestStore
{
    public static IWeightedTownHallManifestStore NewMemoryWeightedTownHallManifestStore()
    {
        return new MemoryWeightedTownHallManifestStore(
            new Dictionary<string, WeightedTownHallManifest>(),
            new List<string>());
    }

    public static IWeightedTownHallManifestStore NewCachedWeightedTownHallManifestStore(MemoryWeightedTownHallManifestStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedTownHallManifestStore(
            inner,
            WeightedTownHallManifestCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<WeightedTownHallManifest> FindWeightedTownHallManifests(IWeightedTownHallManifestStore store, WeightedTownHallManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
