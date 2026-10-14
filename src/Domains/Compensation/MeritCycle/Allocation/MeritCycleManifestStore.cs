namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle manifests by code.</summary>
public interface IMeritCycleManifestStore
{
    MeritCycleManifest? Find(string code);
    void Save(MeritCycleManifest item);
    bool Remove(string code);
    List<MeritCycleManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleManifestStore(Dictionary<string, MeritCycleManifest> items, List<string> order) : IMeritCycleManifestStore
{
    public Dictionary<string, MeritCycleManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleManifest item)
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

    public List<MeritCycleManifest> List()
    {
        var outValue = new List<MeritCycleManifest>();
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
public sealed class CachedMeritCycleManifestStore(MemoryMeritCycleManifestStore inner, MeritCycleManifestCache cache, int hits, int misses) : IMeritCycleManifestStore
{
    public MemoryMeritCycleManifestStore Inner { get; set; } = inner;
    public MeritCycleManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleManifest? Find(string code)
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

    public void Save(MeritCycleManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle manifests. Empty fields match all.</summary>
public sealed record MeritCycleManifestQuery(string Title, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleManifest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleManifest> Apply(List<MeritCycleManifest> items)
    {
        var matched = Collections.Filter(items, (MeritCycleManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle manifest store.</summary>
public static class MeritCycleManifestStore
{
    public static IMeritCycleManifestStore NewMemoryMeritCycleManifestStore()
    {
        return new MemoryMeritCycleManifestStore(new Dictionary<string, MeritCycleManifest>(), new List<string>());
    }

    public static IMeritCycleManifestStore NewCachedMeritCycleManifestStore(MemoryMeritCycleManifestStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleManifestStore(inner, MeritCycleManifestCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<MeritCycleManifest> FindMeritCycleManifests(IMeritCycleManifestStore store, MeritCycleManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
