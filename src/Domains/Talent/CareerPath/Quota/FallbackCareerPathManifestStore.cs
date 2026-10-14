namespace Acme.Workforce.Talent.CareerPath.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback career path manifests by code.</summary>
public interface IFallbackCareerPathManifestStore
{
    FallbackCareerPathManifest? Find(string code);
    void Save(FallbackCareerPathManifest item);
    bool Remove(string code);
    List<FallbackCareerPathManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackCareerPathManifestStore(Dictionary<string, FallbackCareerPathManifest> items, List<string> order) : IFallbackCareerPathManifestStore
{
    public Dictionary<string, FallbackCareerPathManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackCareerPathManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackCareerPathManifest item)
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

    public List<FallbackCareerPathManifest> List()
    {
        var outValue = new List<FallbackCareerPathManifest>();
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
public sealed class CachedFallbackCareerPathManifestStore(MemoryFallbackCareerPathManifestStore inner, FallbackCareerPathManifestCache cache, int hits, int misses) : IFallbackCareerPathManifestStore
{
    public MemoryFallbackCareerPathManifestStore Inner { get; set; } = inner;
    public FallbackCareerPathManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackCareerPathManifest? Find(string code)
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

    public void Save(FallbackCareerPathManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackCareerPathManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback career path manifests. Empty fields match all.</summary>
public sealed record FallbackCareerPathManifestQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackCareerPathManifest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackCareerPathManifest> Apply(List<FallbackCareerPathManifest> items)
    {
        var matched = Collections.Filter(items, (FallbackCareerPathManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback career path manifest store.</summary>
public static class FallbackCareerPathManifestStore
{
    public static IFallbackCareerPathManifestStore NewMemoryFallbackCareerPathManifestStore()
    {
        return new MemoryFallbackCareerPathManifestStore(
            new Dictionary<string, FallbackCareerPathManifest>(),
            new List<string>());
    }

    public static IFallbackCareerPathManifestStore NewCachedFallbackCareerPathManifestStore(MemoryFallbackCareerPathManifestStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackCareerPathManifestStore(
            inner,
            FallbackCareerPathManifestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<FallbackCareerPathManifest> FindFallbackCareerPathManifests(IFallbackCareerPathManifestStore store, FallbackCareerPathManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
