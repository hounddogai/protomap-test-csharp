namespace Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback succession plan manifests by code.</summary>
public interface IFallbackSuccessionPlanManifestStore
{
    FallbackSuccessionPlanManifest? Find(string code);
    void Save(FallbackSuccessionPlanManifest item);
    bool Remove(string code);
    List<FallbackSuccessionPlanManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackSuccessionPlanManifestStore(Dictionary<string, FallbackSuccessionPlanManifest> items, List<string> order) : IFallbackSuccessionPlanManifestStore
{
    public Dictionary<string, FallbackSuccessionPlanManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackSuccessionPlanManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackSuccessionPlanManifest item)
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

    public List<FallbackSuccessionPlanManifest> List()
    {
        var outValue = new List<FallbackSuccessionPlanManifest>();
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
public sealed class CachedFallbackSuccessionPlanManifestStore(MemoryFallbackSuccessionPlanManifestStore inner, FallbackSuccessionPlanManifestCache cache, int hits, int misses) : IFallbackSuccessionPlanManifestStore
{
    public MemoryFallbackSuccessionPlanManifestStore Inner { get; set; } = inner;
    public FallbackSuccessionPlanManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackSuccessionPlanManifest? Find(string code)
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

    public void Save(FallbackSuccessionPlanManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackSuccessionPlanManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback succession plan manifests. Empty fields match all.</summary>
public sealed record FallbackSuccessionPlanManifestQuery(string Name, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackSuccessionPlanManifest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<FallbackSuccessionPlanManifest> Apply(List<FallbackSuccessionPlanManifest> items)
    {
        var matched = Collections.Filter(items, (FallbackSuccessionPlanManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback succession plan manifest store.</summary>
public static class FallbackSuccessionPlanManifestStore
{
    public static IFallbackSuccessionPlanManifestStore NewMemoryFallbackSuccessionPlanManifestStore()
    {
        return new MemoryFallbackSuccessionPlanManifestStore(
            new Dictionary<string, FallbackSuccessionPlanManifest>(),
            new List<string>());
    }

    public static IFallbackSuccessionPlanManifestStore NewCachedFallbackSuccessionPlanManifestStore(MemoryFallbackSuccessionPlanManifestStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackSuccessionPlanManifestStore(
            inner,
            FallbackSuccessionPlanManifestCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackSuccessionPlanManifest> FindFallbackSuccessionPlanManifests(IFallbackSuccessionPlanManifestStore store, FallbackSuccessionPlanManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
