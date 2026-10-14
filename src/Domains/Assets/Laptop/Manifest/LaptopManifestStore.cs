namespace Acme.Workforce.Assets.Laptop.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop manifests by code.</summary>
public interface ILaptopManifestStore
{
    LaptopManifest? Find(string code);
    void Save(LaptopManifest item);
    bool Remove(string code);
    List<LaptopManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopManifestStore(Dictionary<string, LaptopManifest> items, List<string> order) : ILaptopManifestStore
{
    public Dictionary<string, LaptopManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopManifest item)
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

    public List<LaptopManifest> List()
    {
        var outValue = new List<LaptopManifest>();
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
public sealed class CachedLaptopManifestStore(MemoryLaptopManifestStore inner, LaptopManifestCache cache, int hits, int misses) : ILaptopManifestStore
{
    public MemoryLaptopManifestStore Inner { get; set; } = inner;
    public LaptopManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopManifest? Find(string code)
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

    public void Save(LaptopManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop manifests. Empty fields match all.</summary>
public sealed record LaptopManifestQuery(string Team, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopManifest item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopManifest> Apply(List<LaptopManifest> items)
    {
        var matched = Collections.Filter(items, (LaptopManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop manifest store.</summary>
public static class LaptopManifestStore
{
    public static ILaptopManifestStore NewMemoryLaptopManifestStore()
    {
        return new MemoryLaptopManifestStore(new Dictionary<string, LaptopManifest>(), new List<string>());
    }

    public static ILaptopManifestStore NewCachedLaptopManifestStore(MemoryLaptopManifestStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopManifestStore(inner, LaptopManifestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LaptopManifest> FindLaptopManifests(ILaptopManifestStore store, LaptopManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
