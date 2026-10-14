namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy cost model manifests by code.</summary>
public interface ILegacyCostModelManifestStore
{
    LegacyCostModelManifest? Find(string code);
    void Save(LegacyCostModelManifest item);
    bool Remove(string code);
    List<LegacyCostModelManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyCostModelManifestStore(Dictionary<string, LegacyCostModelManifest> items, List<string> order) : ILegacyCostModelManifestStore
{
    public Dictionary<string, LegacyCostModelManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyCostModelManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyCostModelManifest item)
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

    public List<LegacyCostModelManifest> List()
    {
        var outValue = new List<LegacyCostModelManifest>();
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
public sealed class CachedLegacyCostModelManifestStore(MemoryLegacyCostModelManifestStore inner, LegacyCostModelManifestCache cache, int hits, int misses) : ILegacyCostModelManifestStore
{
    public MemoryLegacyCostModelManifestStore Inner { get; set; } = inner;
    public LegacyCostModelManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyCostModelManifest? Find(string code)
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

    public void Save(LegacyCostModelManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyCostModelManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy cost model manifests. Empty fields match all.</summary>
public sealed record LegacyCostModelManifestQuery(string Title, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyCostModelManifest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyCostModelManifest> Apply(List<LegacyCostModelManifest> items)
    {
        var matched = Collections.Filter(items, (LegacyCostModelManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy cost model manifest store.</summary>
public static class LegacyCostModelManifestStore
{
    public static ILegacyCostModelManifestStore NewMemoryLegacyCostModelManifestStore()
    {
        return new MemoryLegacyCostModelManifestStore(
            new Dictionary<string, LegacyCostModelManifest>(),
            new List<string>());
    }

    public static ILegacyCostModelManifestStore NewCachedLegacyCostModelManifestStore(MemoryLegacyCostModelManifestStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyCostModelManifestStore(
            inner,
            LegacyCostModelManifestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LegacyCostModelManifest> FindLegacyCostModelManifests(ILegacyCostModelManifestStore store, LegacyCostModelManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
