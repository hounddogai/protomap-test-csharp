namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment manifests by code.</summary>
public interface IAdjustmentManifestStore
{
    AdjustmentManifest? Find(string code);
    void Save(AdjustmentManifest item);
    bool Remove(string code);
    List<AdjustmentManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentManifestStore(Dictionary<string, AdjustmentManifest> items, List<string> order) : IAdjustmentManifestStore
{
    public Dictionary<string, AdjustmentManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentManifest item)
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

    public List<AdjustmentManifest> List()
    {
        var outValue = new List<AdjustmentManifest>();
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
public sealed class CachedAdjustmentManifestStore(MemoryAdjustmentManifestStore inner, AdjustmentManifestCache cache, int hits, int misses) : IAdjustmentManifestStore
{
    public MemoryAdjustmentManifestStore Inner { get; set; } = inner;
    public AdjustmentManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentManifest? Find(string code)
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

    public void Save(AdjustmentManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment manifests. Empty fields match all.</summary>
public sealed record AdjustmentManifestQuery(string Title, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentManifest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<AdjustmentManifest> Apply(List<AdjustmentManifest> items)
    {
        var matched = Collections.Filter(items, (AdjustmentManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment manifest store.</summary>
public static class AdjustmentManifestStore
{
    public static IAdjustmentManifestStore NewMemoryAdjustmentManifestStore()
    {
        return new MemoryAdjustmentManifestStore(new Dictionary<string, AdjustmentManifest>(), new List<string>());
    }

    public static IAdjustmentManifestStore NewCachedAdjustmentManifestStore(MemoryAdjustmentManifestStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentManifestStore(inner, AdjustmentManifestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AdjustmentManifest> FindAdjustmentManifests(IAdjustmentManifestStore store, AdjustmentManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
