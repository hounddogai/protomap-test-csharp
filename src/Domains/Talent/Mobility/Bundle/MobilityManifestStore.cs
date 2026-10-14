namespace Acme.Workforce.Talent.Mobility.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility manifests by code.</summary>
public interface IMobilityManifestStore
{
    MobilityManifest? Find(string code);
    void Save(MobilityManifest item);
    bool Remove(string code);
    List<MobilityManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityManifestStore(Dictionary<string, MobilityManifest> items, List<string> order) : IMobilityManifestStore
{
    public Dictionary<string, MobilityManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityManifest item)
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

    public List<MobilityManifest> List()
    {
        var outValue = new List<MobilityManifest>();
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
public sealed class CachedMobilityManifestStore(MemoryMobilityManifestStore inner, MobilityManifestCache cache, int hits, int misses) : IMobilityManifestStore
{
    public MemoryMobilityManifestStore Inner { get; set; } = inner;
    public MobilityManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityManifest? Find(string code)
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

    public void Save(MobilityManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility manifests. Empty fields match all.</summary>
public sealed record MobilityManifestQuery(string Name, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityManifest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MobilityManifest> Apply(List<MobilityManifest> items)
    {
        var matched = Collections.Filter(items, (MobilityManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility manifest store.</summary>
public static class MobilityManifestStore
{
    public static IMobilityManifestStore NewMemoryMobilityManifestStore()
    {
        return new MemoryMobilityManifestStore(new Dictionary<string, MobilityManifest>(), new List<string>());
    }

    public static IMobilityManifestStore NewCachedMobilityManifestStore(MemoryMobilityManifestStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityManifestStore(inner, MobilityManifestCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<MobilityManifest> FindMobilityManifests(IMobilityManifestStore store, MobilityManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
