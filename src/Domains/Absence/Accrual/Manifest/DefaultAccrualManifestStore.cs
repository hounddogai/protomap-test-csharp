namespace Acme.Workforce.Absence.Accrual.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default accrual manifests by code.</summary>
public interface IDefaultAccrualManifestStore
{
    DefaultAccrualManifest? Find(string code);
    void Save(DefaultAccrualManifest item);
    bool Remove(string code);
    List<DefaultAccrualManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultAccrualManifestStore(Dictionary<string, DefaultAccrualManifest> items, List<string> order) : IDefaultAccrualManifestStore
{
    public Dictionary<string, DefaultAccrualManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultAccrualManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultAccrualManifest item)
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

    public List<DefaultAccrualManifest> List()
    {
        var outValue = new List<DefaultAccrualManifest>();
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
public sealed class CachedDefaultAccrualManifestStore(MemoryDefaultAccrualManifestStore inner, DefaultAccrualManifestCache cache, int hits, int misses) : IDefaultAccrualManifestStore
{
    public MemoryDefaultAccrualManifestStore Inner { get; set; } = inner;
    public DefaultAccrualManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultAccrualManifest? Find(string code)
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

    public void Save(DefaultAccrualManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultAccrualManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default accrual manifests. Empty fields match all.</summary>
public sealed record DefaultAccrualManifestQuery(string Unit, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultAccrualManifest item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultAccrualManifest> Apply(List<DefaultAccrualManifest> items)
    {
        var matched = Collections.Filter(items, (DefaultAccrualManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default accrual manifest store.</summary>
public static class DefaultAccrualManifestStore
{
    public static IDefaultAccrualManifestStore NewMemoryDefaultAccrualManifestStore()
    {
        return new MemoryDefaultAccrualManifestStore(
            new Dictionary<string, DefaultAccrualManifest>(),
            new List<string>());
    }

    public static IDefaultAccrualManifestStore NewCachedDefaultAccrualManifestStore(MemoryDefaultAccrualManifestStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultAccrualManifestStore(
            inner,
            DefaultAccrualManifestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<DefaultAccrualManifest> FindDefaultAccrualManifests(IDefaultAccrualManifestStore store, DefaultAccrualManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
