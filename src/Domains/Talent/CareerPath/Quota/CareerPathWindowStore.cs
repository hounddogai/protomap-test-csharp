namespace Acme.Workforce.Talent.CareerPath.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path windows by code.</summary>
public interface ICareerPathWindowStore
{
    CareerPathWindow? Find(string code);
    void Save(CareerPathWindow item);
    bool Remove(string code);
    List<CareerPathWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathWindowStore(Dictionary<string, CareerPathWindow> items, List<string> order) : ICareerPathWindowStore
{
    public Dictionary<string, CareerPathWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathWindow item)
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

    public List<CareerPathWindow> List()
    {
        var outValue = new List<CareerPathWindow>();
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
public sealed class CachedCareerPathWindowStore(MemoryCareerPathWindowStore inner, CareerPathWindowCache cache, int hits, int misses) : ICareerPathWindowStore
{
    public MemoryCareerPathWindowStore Inner { get; set; } = inner;
    public CareerPathWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathWindow? Find(string code)
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

    public void Save(CareerPathWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path windows. Empty fields match all.</summary>
public sealed record CareerPathWindowQuery(string Team, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathWindow item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathWindow> Apply(List<CareerPathWindow> items)
    {
        var matched = Collections.Filter(items, (CareerPathWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path window store.</summary>
public static class CareerPathWindowStore
{
    public static ICareerPathWindowStore NewMemoryCareerPathWindowStore()
    {
        return new MemoryCareerPathWindowStore(new Dictionary<string, CareerPathWindow>(), new List<string>());
    }

    public static ICareerPathWindowStore NewCachedCareerPathWindowStore(MemoryCareerPathWindowStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathWindowStore(inner, CareerPathWindowCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CareerPathWindow> FindCareerPathWindows(ICareerPathWindowStore store, CareerPathWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
