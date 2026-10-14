namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient occupancy plans by code.</summary>
public interface ILenientOccupancyPlanStore
{
    LenientOccupancyPlan? Find(string code);
    void Save(LenientOccupancyPlan item);
    bool Remove(string code);
    List<LenientOccupancyPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientOccupancyPlanStore(Dictionary<string, LenientOccupancyPlan> items, List<string> order) : ILenientOccupancyPlanStore
{
    public Dictionary<string, LenientOccupancyPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientOccupancyPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientOccupancyPlan item)
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

    public List<LenientOccupancyPlan> List()
    {
        var outValue = new List<LenientOccupancyPlan>();
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
public sealed class CachedLenientOccupancyPlanStore(MemoryLenientOccupancyPlanStore inner, LenientOccupancyPlanCache cache, int hits, int misses) : ILenientOccupancyPlanStore
{
    public MemoryLenientOccupancyPlanStore Inner { get; set; } = inner;
    public LenientOccupancyPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientOccupancyPlan? Find(string code)
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

    public void Save(LenientOccupancyPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientOccupancyPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient occupancy plans. Empty fields match all.</summary>
public sealed record LenientOccupancyPlanQuery(string Title, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(LenientOccupancyPlan item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientOccupancyPlan> Apply(List<LenientOccupancyPlan> items)
    {
        var matched = Collections.Filter(items, (LenientOccupancyPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient occupancy plan store.</summary>
public static class LenientOccupancyPlanStore
{
    public static ILenientOccupancyPlanStore NewMemoryLenientOccupancyPlanStore()
    {
        return new MemoryLenientOccupancyPlanStore(
            new Dictionary<string, LenientOccupancyPlan>(),
            new List<string>());
    }

    public static ILenientOccupancyPlanStore NewCachedLenientOccupancyPlanStore(MemoryLenientOccupancyPlanStore inner, int ttl, IClock clock)
    {
        return new CachedLenientOccupancyPlanStore(inner, LenientOccupancyPlanCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LenientOccupancyPlan> FindLenientOccupancyPlans(ILenientOccupancyPlanStore store, LenientOccupancyPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
