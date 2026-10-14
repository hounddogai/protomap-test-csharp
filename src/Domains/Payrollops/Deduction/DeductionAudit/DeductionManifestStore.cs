namespace Acme.Workforce.Payrollops.Deduction.DeductionAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction manifests by code.</summary>
public interface IDeductionManifestStore
{
    DeductionManifest? Find(string code);
    void Save(DeductionManifest item);
    bool Remove(string code);
    List<DeductionManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionManifestStore(Dictionary<string, DeductionManifest> items, List<string> order) : IDeductionManifestStore
{
    public Dictionary<string, DeductionManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionManifest item)
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

    public List<DeductionManifest> List()
    {
        var outValue = new List<DeductionManifest>();
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
public sealed class CachedDeductionManifestStore(MemoryDeductionManifestStore inner, DeductionManifestCache cache, int hits, int misses) : IDeductionManifestStore
{
    public MemoryDeductionManifestStore Inner { get; set; } = inner;
    public DeductionManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionManifest? Find(string code)
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

    public void Save(DeductionManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction manifests. Empty fields match all.</summary>
public sealed record DeductionManifestQuery(string Source, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionManifest item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionManifest> Apply(List<DeductionManifest> items)
    {
        var matched = Collections.Filter(items, (DeductionManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction manifest store.</summary>
public static class DeductionManifestStore
{
    public static IDeductionManifestStore NewMemoryDeductionManifestStore()
    {
        return new MemoryDeductionManifestStore(new Dictionary<string, DeductionManifest>(), new List<string>());
    }

    public static IDeductionManifestStore NewCachedDeductionManifestStore(MemoryDeductionManifestStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionManifestStore(inner, DeductionManifestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DeductionManifest> FindDeductionManifests(IDeductionManifestStore store, DeductionManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
