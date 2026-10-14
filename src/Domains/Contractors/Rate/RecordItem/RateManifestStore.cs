namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate manifests by code.</summary>
public interface IRateManifestStore
{
    RateManifest? Find(string code);
    void Save(RateManifest item);
    bool Remove(string code);
    List<RateManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateManifestStore(Dictionary<string, RateManifest> items, List<string> order) : IRateManifestStore
{
    public Dictionary<string, RateManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateManifest item)
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

    public List<RateManifest> List()
    {
        var outValue = new List<RateManifest>();
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
public sealed class CachedRateManifestStore(MemoryRateManifestStore inner, RateManifestCache cache, int hits, int misses) : IRateManifestStore
{
    public MemoryRateManifestStore Inner { get; set; } = inner;
    public RateManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateManifest? Find(string code)
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

    public void Save(RateManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate manifests. Empty fields match all.</summary>
public sealed record RateManifestQuery(string Category, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(RateManifest item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<RateManifest> Apply(List<RateManifest> items)
    {
        var matched = Collections.Filter(items, (RateManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate manifest store.</summary>
public static class RateManifestStore
{
    public static IRateManifestStore NewMemoryRateManifestStore()
    {
        return new MemoryRateManifestStore(new Dictionary<string, RateManifest>(), new List<string>());
    }

    public static IRateManifestStore NewCachedRateManifestStore(MemoryRateManifestStore inner, int ttl, IClock clock)
    {
        return new CachedRateManifestStore(inner, RateManifestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RateManifest> FindRateManifests(IRateManifestStore store, RateManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
