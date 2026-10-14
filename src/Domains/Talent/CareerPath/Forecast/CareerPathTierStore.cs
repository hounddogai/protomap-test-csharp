namespace Acme.Workforce.Talent.CareerPath.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path tiers by code.</summary>
public interface ICareerPathTierStore
{
    CareerPathTier? Find(string code);
    void Save(CareerPathTier item);
    bool Remove(string code);
    List<CareerPathTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathTierStore(Dictionary<string, CareerPathTier> items, List<string> order) : ICareerPathTierStore
{
    public Dictionary<string, CareerPathTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathTier item)
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

    public List<CareerPathTier> List()
    {
        var outValue = new List<CareerPathTier>();
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
public sealed class CachedCareerPathTierStore(MemoryCareerPathTierStore inner, CareerPathTierCache cache, int hits, int misses) : ICareerPathTierStore
{
    public MemoryCareerPathTierStore Inner { get; set; } = inner;
    public CareerPathTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathTier? Find(string code)
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

    public void Save(CareerPathTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path tiers. Empty fields match all.</summary>
public sealed record CareerPathTierQuery(string Locale, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathTier item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathTier> Apply(List<CareerPathTier> items)
    {
        var matched = Collections.Filter(items, (CareerPathTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path tier store.</summary>
public static class CareerPathTierStore
{
    public static ICareerPathTierStore NewMemoryCareerPathTierStore()
    {
        return new MemoryCareerPathTierStore(new Dictionary<string, CareerPathTier>(), new List<string>());
    }

    public static ICareerPathTierStore NewCachedCareerPathTierStore(MemoryCareerPathTierStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathTierStore(inner, CareerPathTierCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CareerPathTier> FindCareerPathTiers(ICareerPathTierStore store, CareerPathTierQuery query)
    {
        return query.Apply(store.List());
    }
}
