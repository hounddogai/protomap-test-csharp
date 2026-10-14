namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped laptop manifests by code.</summary>
public interface ICappedLaptopManifestStore
{
    CappedLaptopManifest? Find(string code);
    void Save(CappedLaptopManifest item);
    bool Remove(string code);
    List<CappedLaptopManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedLaptopManifestStore(Dictionary<string, CappedLaptopManifest> items, List<string> order) : ICappedLaptopManifestStore
{
    public Dictionary<string, CappedLaptopManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedLaptopManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedLaptopManifest item)
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

    public List<CappedLaptopManifest> List()
    {
        var outValue = new List<CappedLaptopManifest>();
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
public sealed class CachedCappedLaptopManifestStore(MemoryCappedLaptopManifestStore inner, CappedLaptopManifestCache cache, int hits, int misses) : ICappedLaptopManifestStore
{
    public MemoryCappedLaptopManifestStore Inner { get; set; } = inner;
    public CappedLaptopManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedLaptopManifest? Find(string code)
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

    public void Save(CappedLaptopManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedLaptopManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped laptop manifests. Empty fields match all.</summary>
public sealed record CappedLaptopManifestQuery(string Name, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(CappedLaptopManifest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<CappedLaptopManifest> Apply(List<CappedLaptopManifest> items)
    {
        var matched = Collections.Filter(items, (CappedLaptopManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped laptop manifest store.</summary>
public static class CappedLaptopManifestStore
{
    public static ICappedLaptopManifestStore NewMemoryCappedLaptopManifestStore()
    {
        return new MemoryCappedLaptopManifestStore(
            new Dictionary<string, CappedLaptopManifest>(),
            new List<string>());
    }

    public static ICappedLaptopManifestStore NewCachedCappedLaptopManifestStore(MemoryCappedLaptopManifestStore inner, int ttl, IClock clock)
    {
        return new CachedCappedLaptopManifestStore(inner, CappedLaptopManifestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CappedLaptopManifest> FindCappedLaptopManifests(ICappedLaptopManifestStore store, CappedLaptopManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
