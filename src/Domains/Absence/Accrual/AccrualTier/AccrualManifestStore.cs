namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual manifests by code.</summary>
public interface IAccrualManifestStore
{
    AccrualManifest? Find(string code);
    void Save(AccrualManifest item);
    bool Remove(string code);
    List<AccrualManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualManifestStore(Dictionary<string, AccrualManifest> items, List<string> order) : IAccrualManifestStore
{
    public Dictionary<string, AccrualManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualManifest item)
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

    public List<AccrualManifest> List()
    {
        var outValue = new List<AccrualManifest>();
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
public sealed class CachedAccrualManifestStore(MemoryAccrualManifestStore inner, AccrualManifestCache cache, int hits, int misses) : IAccrualManifestStore
{
    public MemoryAccrualManifestStore Inner { get; set; } = inner;
    public AccrualManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualManifest? Find(string code)
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

    public void Save(AccrualManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual manifests. Empty fields match all.</summary>
public sealed record AccrualManifestQuery(string Label, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualManifest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualManifest> Apply(List<AccrualManifest> items)
    {
        var matched = Collections.Filter(items, (AccrualManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual manifest store.</summary>
public static class AccrualManifestStore
{
    public static IAccrualManifestStore NewMemoryAccrualManifestStore()
    {
        return new MemoryAccrualManifestStore(new Dictionary<string, AccrualManifest>(), new List<string>());
    }

    public static IAccrualManifestStore NewCachedAccrualManifestStore(MemoryAccrualManifestStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualManifestStore(inner, AccrualManifestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AccrualManifest> FindAccrualManifests(IAccrualManifestStore store, AccrualManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
