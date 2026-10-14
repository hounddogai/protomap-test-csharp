namespace Acme.Workforce.Absence.Accrual.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped accrual manifests by code.</summary>
public interface ICappedAccrualManifestStore
{
    CappedAccrualManifest? Find(string code);
    void Save(CappedAccrualManifest item);
    bool Remove(string code);
    List<CappedAccrualManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedAccrualManifestStore(Dictionary<string, CappedAccrualManifest> items, List<string> order) : ICappedAccrualManifestStore
{
    public Dictionary<string, CappedAccrualManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedAccrualManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedAccrualManifest item)
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

    public List<CappedAccrualManifest> List()
    {
        var outValue = new List<CappedAccrualManifest>();
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
public sealed class CachedCappedAccrualManifestStore(MemoryCappedAccrualManifestStore inner, CappedAccrualManifestCache cache, int hits, int misses) : ICappedAccrualManifestStore
{
    public MemoryCappedAccrualManifestStore Inner { get; set; } = inner;
    public CappedAccrualManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedAccrualManifest? Find(string code)
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

    public void Save(CappedAccrualManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedAccrualManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped accrual manifests. Empty fields match all.</summary>
public sealed record CappedAccrualManifestQuery(string Label, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(CappedAccrualManifest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedAccrualManifest> Apply(List<CappedAccrualManifest> items)
    {
        var matched = Collections.Filter(items, (CappedAccrualManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped accrual manifest store.</summary>
public static class CappedAccrualManifestStore
{
    public static ICappedAccrualManifestStore NewMemoryCappedAccrualManifestStore()
    {
        return new MemoryCappedAccrualManifestStore(
            new Dictionary<string, CappedAccrualManifest>(),
            new List<string>());
    }

    public static ICappedAccrualManifestStore NewCachedCappedAccrualManifestStore(MemoryCappedAccrualManifestStore inner, int ttl, IClock clock)
    {
        return new CachedCappedAccrualManifestStore(
            inner,
            CappedAccrualManifestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedAccrualManifest> FindCappedAccrualManifests(ICappedAccrualManifestStore store, CappedAccrualManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
