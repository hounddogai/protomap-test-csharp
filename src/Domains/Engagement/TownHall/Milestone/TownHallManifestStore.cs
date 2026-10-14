namespace Acme.Workforce.Engagement.TownHall.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall manifests by code.</summary>
public interface ITownHallManifestStore
{
    TownHallManifest? Find(string code);
    void Save(TownHallManifest item);
    bool Remove(string code);
    List<TownHallManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallManifestStore(Dictionary<string, TownHallManifest> items, List<string> order) : ITownHallManifestStore
{
    public Dictionary<string, TownHallManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallManifest item)
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

    public List<TownHallManifest> List()
    {
        var outValue = new List<TownHallManifest>();
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
public sealed class CachedTownHallManifestStore(MemoryTownHallManifestStore inner, TownHallManifestCache cache, int hits, int misses) : ITownHallManifestStore
{
    public MemoryTownHallManifestStore Inner { get; set; } = inner;
    public TownHallManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallManifest? Find(string code)
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

    public void Save(TownHallManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall manifests. Empty fields match all.</summary>
public sealed record TownHallManifestQuery(string Name, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallManifest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallManifest> Apply(List<TownHallManifest> items)
    {
        var matched = Collections.Filter(items, (TownHallManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall manifest store.</summary>
public static class TownHallManifestStore
{
    public static ITownHallManifestStore NewMemoryTownHallManifestStore()
    {
        return new MemoryTownHallManifestStore(new Dictionary<string, TownHallManifest>(), new List<string>());
    }

    public static ITownHallManifestStore NewCachedTownHallManifestStore(MemoryTownHallManifestStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallManifestStore(inner, TownHallManifestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TownHallManifest> FindTownHallManifests(ITownHallManifestStore store, TownHallManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
