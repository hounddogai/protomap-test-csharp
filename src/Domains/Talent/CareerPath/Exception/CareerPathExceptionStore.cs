namespace Acme.Workforce.Talent.CareerPath.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path exceptions by code.</summary>
public interface ICareerPathExceptionStore
{
    CareerPathException? Find(string code);
    void Save(CareerPathException item);
    bool Remove(string code);
    List<CareerPathException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathExceptionStore(Dictionary<string, CareerPathException> items, List<string> order) : ICareerPathExceptionStore
{
    public Dictionary<string, CareerPathException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathException item)
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

    public List<CareerPathException> List()
    {
        var outValue = new List<CareerPathException>();
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
public sealed class CachedCareerPathExceptionStore(MemoryCareerPathExceptionStore inner, CareerPathExceptionCache cache, int hits, int misses) : ICareerPathExceptionStore
{
    public MemoryCareerPathExceptionStore Inner { get; set; } = inner;
    public CareerPathExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathException? Find(string code)
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

    public void Save(CareerPathException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path exceptions. Empty fields match all.</summary>
public sealed record CareerPathExceptionQuery(string Country, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathException item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathException> Apply(List<CareerPathException> items)
    {
        var matched = Collections.Filter(items, (CareerPathException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path exception store.</summary>
public static class CareerPathExceptionStore
{
    public static ICareerPathExceptionStore NewMemoryCareerPathExceptionStore()
    {
        return new MemoryCareerPathExceptionStore(new Dictionary<string, CareerPathException>(), new List<string>());
    }

    public static ICareerPathExceptionStore NewCachedCareerPathExceptionStore(MemoryCareerPathExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathExceptionStore(inner, CareerPathExceptionCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<CareerPathException> FindCareerPathExceptions(ICareerPathExceptionStore store, CareerPathExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
