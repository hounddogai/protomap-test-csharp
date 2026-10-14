namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive town hall manifests by code.</summary>
public interface IAdaptiveTownHallManifestStore
{
    AdaptiveTownHallManifest? Find(string code);
    void Save(AdaptiveTownHallManifest item);
    bool Remove(string code);
    List<AdaptiveTownHallManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveTownHallManifestStore(Dictionary<string, AdaptiveTownHallManifest> items, List<string> order) : IAdaptiveTownHallManifestStore
{
    public Dictionary<string, AdaptiveTownHallManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveTownHallManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveTownHallManifest item)
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

    public List<AdaptiveTownHallManifest> List()
    {
        var outValue = new List<AdaptiveTownHallManifest>();
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
public sealed class CachedAdaptiveTownHallManifestStore(MemoryAdaptiveTownHallManifestStore inner, AdaptiveTownHallManifestCache cache, int hits, int misses) : IAdaptiveTownHallManifestStore
{
    public MemoryAdaptiveTownHallManifestStore Inner { get; set; } = inner;
    public AdaptiveTownHallManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveTownHallManifest? Find(string code)
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

    public void Save(AdaptiveTownHallManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveTownHallManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive town hall manifests. Empty fields match all.</summary>
public sealed record AdaptiveTownHallManifestQuery(string Team, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveTownHallManifest item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveTownHallManifest> Apply(List<AdaptiveTownHallManifest> items)
    {
        var matched = Collections.Filter(items, (AdaptiveTownHallManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive town hall manifest store.</summary>
public static class AdaptiveTownHallManifestStore
{
    public static IAdaptiveTownHallManifestStore NewMemoryAdaptiveTownHallManifestStore()
    {
        return new MemoryAdaptiveTownHallManifestStore(
            new Dictionary<string, AdaptiveTownHallManifest>(),
            new List<string>());
    }

    public static IAdaptiveTownHallManifestStore NewCachedAdaptiveTownHallManifestStore(MemoryAdaptiveTownHallManifestStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveTownHallManifestStore(
            inner,
            AdaptiveTownHallManifestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveTownHallManifest> FindAdaptiveTownHallManifests(IAdaptiveTownHallManifestStore store, AdaptiveTownHallManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
