namespace Acme.Workforce.Talent.CareerPath.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path plans by code.</summary>
public interface ICareerPathPlanStore
{
    CareerPathPlan? Find(string code);
    void Save(CareerPathPlan item);
    bool Remove(string code);
    List<CareerPathPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathPlanStore(Dictionary<string, CareerPathPlan> items, List<string> order) : ICareerPathPlanStore
{
    public Dictionary<string, CareerPathPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathPlan item)
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

    public List<CareerPathPlan> List()
    {
        var outValue = new List<CareerPathPlan>();
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
public sealed class CachedCareerPathPlanStore(MemoryCareerPathPlanStore inner, CareerPathPlanCache cache, int hits, int misses) : ICareerPathPlanStore
{
    public MemoryCareerPathPlanStore Inner { get; set; } = inner;
    public CareerPathPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathPlan? Find(string code)
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

    public void Save(CareerPathPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path plans. Empty fields match all.</summary>
public sealed record CareerPathPlanQuery(string Category, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathPlan item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathPlan> Apply(List<CareerPathPlan> items)
    {
        var matched = Collections.Filter(items, (CareerPathPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path plan store.</summary>
public static class CareerPathPlanStore
{
    public static ICareerPathPlanStore NewMemoryCareerPathPlanStore()
    {
        return new MemoryCareerPathPlanStore(new Dictionary<string, CareerPathPlan>(), new List<string>());
    }

    public static ICareerPathPlanStore NewCachedCareerPathPlanStore(MemoryCareerPathPlanStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathPlanStore(inner, CareerPathPlanCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CareerPathPlan> FindCareerPathPlans(ICareerPathPlanStore store, CareerPathPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
