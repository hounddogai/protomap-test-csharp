namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path profiles by code.</summary>
public interface ICareerPathProfileStore
{
    CareerPathProfile? Find(string code);
    void Save(CareerPathProfile item);
    bool Remove(string code);
    List<CareerPathProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathProfileStore(Dictionary<string, CareerPathProfile> items, List<string> order) : ICareerPathProfileStore
{
    public Dictionary<string, CareerPathProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathProfile item)
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

    public List<CareerPathProfile> List()
    {
        var outValue = new List<CareerPathProfile>();
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
public sealed class CachedCareerPathProfileStore(MemoryCareerPathProfileStore inner, CareerPathProfileCache cache, int hits, int misses) : ICareerPathProfileStore
{
    public MemoryCareerPathProfileStore Inner { get; set; } = inner;
    public CareerPathProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathProfile? Find(string code)
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

    public void Save(CareerPathProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path profiles. Empty fields match all.</summary>
public sealed record CareerPathProfileQuery(string Name, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathProfile item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathProfile> Apply(List<CareerPathProfile> items)
    {
        var matched = Collections.Filter(items, (CareerPathProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path profile store.</summary>
public static class CareerPathProfileStore
{
    public static ICareerPathProfileStore NewMemoryCareerPathProfileStore()
    {
        return new MemoryCareerPathProfileStore(new Dictionary<string, CareerPathProfile>(), new List<string>());
    }

    public static ICareerPathProfileStore NewCachedCareerPathProfileStore(MemoryCareerPathProfileStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathProfileStore(inner, CareerPathProfileCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CareerPathProfile> FindCareerPathProfiles(ICareerPathProfileStore store, CareerPathProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
