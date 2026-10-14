namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path targets by code.</summary>
public interface ICareerPathTargetStore
{
    CareerPathTarget? Find(string code);
    void Save(CareerPathTarget item);
    bool Remove(string code);
    List<CareerPathTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathTargetStore(Dictionary<string, CareerPathTarget> items, List<string> order) : ICareerPathTargetStore
{
    public Dictionary<string, CareerPathTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathTarget item)
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

    public List<CareerPathTarget> List()
    {
        var outValue = new List<CareerPathTarget>();
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
public sealed class CachedCareerPathTargetStore(MemoryCareerPathTargetStore inner, CareerPathTargetCache cache, int hits, int misses) : ICareerPathTargetStore
{
    public MemoryCareerPathTargetStore Inner { get; set; } = inner;
    public CareerPathTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathTarget? Find(string code)
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

    public void Save(CareerPathTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path targets. Empty fields match all.</summary>
public sealed record CareerPathTargetQuery(string Title, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathTarget item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathTarget> Apply(List<CareerPathTarget> items)
    {
        var matched = Collections.Filter(items, (CareerPathTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path target store.</summary>
public static class CareerPathTargetStore
{
    public static ICareerPathTargetStore NewMemoryCareerPathTargetStore()
    {
        return new MemoryCareerPathTargetStore(new Dictionary<string, CareerPathTarget>(), new List<string>());
    }

    public static ICareerPathTargetStore NewCachedCareerPathTargetStore(MemoryCareerPathTargetStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathTargetStore(inner, CareerPathTargetCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CareerPathTarget> FindCareerPathTargets(ICareerPathTargetStore store, CareerPathTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
