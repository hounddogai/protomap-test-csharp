namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path variances by code.</summary>
public interface ICareerPathVarianceStore
{
    CareerPathVariance? Find(string code);
    void Save(CareerPathVariance item);
    bool Remove(string code);
    List<CareerPathVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathVarianceStore(Dictionary<string, CareerPathVariance> items, List<string> order) : ICareerPathVarianceStore
{
    public Dictionary<string, CareerPathVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathVariance item)
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

    public List<CareerPathVariance> List()
    {
        var outValue = new List<CareerPathVariance>();
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
public sealed class CachedCareerPathVarianceStore(MemoryCareerPathVarianceStore inner, CareerPathVarianceCache cache, int hits, int misses) : ICareerPathVarianceStore
{
    public MemoryCareerPathVarianceStore Inner { get; set; } = inner;
    public CareerPathVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathVariance? Find(string code)
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

    public void Save(CareerPathVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path variances. Empty fields match all.</summary>
public sealed record CareerPathVarianceQuery(string Category, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathVariance item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathVariance> Apply(List<CareerPathVariance> items)
    {
        var matched = Collections.Filter(items, (CareerPathVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path variance store.</summary>
public static class CareerPathVarianceStore
{
    public static ICareerPathVarianceStore NewMemoryCareerPathVarianceStore()
    {
        return new MemoryCareerPathVarianceStore(new Dictionary<string, CareerPathVariance>(), new List<string>());
    }

    public static ICareerPathVarianceStore NewCachedCareerPathVarianceStore(MemoryCareerPathVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathVarianceStore(inner, CareerPathVarianceCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CareerPathVariance> FindCareerPathVariances(ICareerPathVarianceStore store, CareerPathVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
