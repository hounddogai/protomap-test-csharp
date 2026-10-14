namespace Acme.Workforce.Orgdesign.CostModel.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model manifests by code.</summary>
public interface ICostModelManifestStore
{
    CostModelManifest? Find(string code);
    void Save(CostModelManifest item);
    bool Remove(string code);
    List<CostModelManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelManifestStore(Dictionary<string, CostModelManifest> items, List<string> order) : ICostModelManifestStore
{
    public Dictionary<string, CostModelManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelManifest item)
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

    public List<CostModelManifest> List()
    {
        var outValue = new List<CostModelManifest>();
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
public sealed class CachedCostModelManifestStore(MemoryCostModelManifestStore inner, CostModelManifestCache cache, int hits, int misses) : ICostModelManifestStore
{
    public MemoryCostModelManifestStore Inner { get; set; } = inner;
    public CostModelManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelManifest? Find(string code)
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

    public void Save(CostModelManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model manifests. Empty fields match all.</summary>
public sealed record CostModelManifestQuery(string Unit, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelManifest item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelManifest> Apply(List<CostModelManifest> items)
    {
        var matched = Collections.Filter(items, (CostModelManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model manifest store.</summary>
public static class CostModelManifestStore
{
    public static ICostModelManifestStore NewMemoryCostModelManifestStore()
    {
        return new MemoryCostModelManifestStore(new Dictionary<string, CostModelManifest>(), new List<string>());
    }

    public static ICostModelManifestStore NewCachedCostModelManifestStore(MemoryCostModelManifestStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelManifestStore(inner, CostModelManifestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CostModelManifest> FindCostModelManifests(ICostModelManifestStore store, CostModelManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
