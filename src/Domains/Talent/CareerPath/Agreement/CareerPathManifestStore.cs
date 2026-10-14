namespace Acme.Workforce.Talent.CareerPath.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path manifests by code.</summary>
public interface ICareerPathManifestStore
{
    CareerPathManifest? Find(string code);
    void Save(CareerPathManifest item);
    bool Remove(string code);
    List<CareerPathManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathManifestStore(Dictionary<string, CareerPathManifest> items, List<string> order) : ICareerPathManifestStore
{
    public Dictionary<string, CareerPathManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathManifest item)
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

    public List<CareerPathManifest> List()
    {
        var outValue = new List<CareerPathManifest>();
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
public sealed class CachedCareerPathManifestStore(MemoryCareerPathManifestStore inner, CareerPathManifestCache cache, int hits, int misses) : ICareerPathManifestStore
{
    public MemoryCareerPathManifestStore Inner { get; set; } = inner;
    public CareerPathManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathManifest? Find(string code)
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

    public void Save(CareerPathManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path manifests. Empty fields match all.</summary>
public sealed record CareerPathManifestQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathManifest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathManifest> Apply(List<CareerPathManifest> items)
    {
        var matched = Collections.Filter(items, (CareerPathManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path manifest store.</summary>
public static class CareerPathManifestStore
{
    public static ICareerPathManifestStore NewMemoryCareerPathManifestStore()
    {
        return new MemoryCareerPathManifestStore(new Dictionary<string, CareerPathManifest>(), new List<string>());
    }

    public static ICareerPathManifestStore NewCachedCareerPathManifestStore(MemoryCareerPathManifestStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathManifestStore(inner, CareerPathManifestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<CareerPathManifest> FindCareerPathManifests(ICareerPathManifestStore store, CareerPathManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
