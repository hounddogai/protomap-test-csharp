namespace Acme.Workforce.Workplace.Desk.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global desk manifests by code.</summary>
public interface IGlobalDeskManifestStore
{
    GlobalDeskManifest? Find(string code);
    void Save(GlobalDeskManifest item);
    bool Remove(string code);
    List<GlobalDeskManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalDeskManifestStore(Dictionary<string, GlobalDeskManifest> items, List<string> order) : IGlobalDeskManifestStore
{
    public Dictionary<string, GlobalDeskManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalDeskManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalDeskManifest item)
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

    public List<GlobalDeskManifest> List()
    {
        var outValue = new List<GlobalDeskManifest>();
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
public sealed class CachedGlobalDeskManifestStore(MemoryGlobalDeskManifestStore inner, GlobalDeskManifestCache cache, int hits, int misses) : IGlobalDeskManifestStore
{
    public MemoryGlobalDeskManifestStore Inner { get; set; } = inner;
    public GlobalDeskManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalDeskManifest? Find(string code)
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

    public void Save(GlobalDeskManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalDeskManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global desk manifests. Empty fields match all.</summary>
public sealed record GlobalDeskManifestQuery(string Channel, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalDeskManifest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalDeskManifest> Apply(List<GlobalDeskManifest> items)
    {
        var matched = Collections.Filter(items, (GlobalDeskManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global desk manifest store.</summary>
public static class GlobalDeskManifestStore
{
    public static IGlobalDeskManifestStore NewMemoryGlobalDeskManifestStore()
    {
        return new MemoryGlobalDeskManifestStore(new Dictionary<string, GlobalDeskManifest>(), new List<string>());
    }

    public static IGlobalDeskManifestStore NewCachedGlobalDeskManifestStore(MemoryGlobalDeskManifestStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalDeskManifestStore(inner, GlobalDeskManifestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<GlobalDeskManifest> FindGlobalDeskManifests(IGlobalDeskManifestStore store, GlobalDeskManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
