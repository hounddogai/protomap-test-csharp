namespace Acme.Workforce.Workplace.Occupancy.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy plans by code.</summary>
public interface IOccupancyPlanStore
{
    OccupancyPlan? Find(string code);
    void Save(OccupancyPlan item);
    bool Remove(string code);
    List<OccupancyPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyPlanStore(Dictionary<string, OccupancyPlan> items, List<string> order) : IOccupancyPlanStore
{
    public Dictionary<string, OccupancyPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyPlan item)
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

    public List<OccupancyPlan> List()
    {
        var outValue = new List<OccupancyPlan>();
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
public sealed class CachedOccupancyPlanStore(MemoryOccupancyPlanStore inner, OccupancyPlanCache cache, int hits, int misses) : IOccupancyPlanStore
{
    public MemoryOccupancyPlanStore Inner { get; set; } = inner;
    public OccupancyPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyPlan? Find(string code)
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

    public void Save(OccupancyPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy plans. Empty fields match all.</summary>
public sealed record OccupancyPlanQuery(string Category, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyPlan item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyPlan> Apply(List<OccupancyPlan> items)
    {
        var matched = Collections.Filter(items, (OccupancyPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy plan store.</summary>
public static class OccupancyPlanStore
{
    public static IOccupancyPlanStore NewMemoryOccupancyPlanStore()
    {
        return new MemoryOccupancyPlanStore(new Dictionary<string, OccupancyPlan>(), new List<string>());
    }

    public static IOccupancyPlanStore NewCachedOccupancyPlanStore(MemoryOccupancyPlanStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyPlanStore(inner, OccupancyPlanCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<OccupancyPlan> FindOccupancyPlans(IOccupancyPlanStore store, OccupancyPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
