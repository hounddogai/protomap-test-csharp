namespace Acme.Workforce.Assets.Laptop.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global laptop plans by code.</summary>
public interface IGlobalLaptopPlanStore
{
    GlobalLaptopPlan? Find(string code);
    void Save(GlobalLaptopPlan item);
    bool Remove(string code);
    List<GlobalLaptopPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalLaptopPlanStore(Dictionary<string, GlobalLaptopPlan> items, List<string> order) : IGlobalLaptopPlanStore
{
    public Dictionary<string, GlobalLaptopPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalLaptopPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalLaptopPlan item)
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

    public List<GlobalLaptopPlan> List()
    {
        var outValue = new List<GlobalLaptopPlan>();
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
public sealed class CachedGlobalLaptopPlanStore(MemoryGlobalLaptopPlanStore inner, GlobalLaptopPlanCache cache, int hits, int misses) : IGlobalLaptopPlanStore
{
    public MemoryGlobalLaptopPlanStore Inner { get; set; } = inner;
    public GlobalLaptopPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalLaptopPlan? Find(string code)
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

    public void Save(GlobalLaptopPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalLaptopPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global laptop plans. Empty fields match all.</summary>
public sealed record GlobalLaptopPlanQuery(string Segment, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalLaptopPlan item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<GlobalLaptopPlan> Apply(List<GlobalLaptopPlan> items)
    {
        var matched = Collections.Filter(items, (GlobalLaptopPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global laptop plan store.</summary>
public static class GlobalLaptopPlanStore
{
    public static IGlobalLaptopPlanStore NewMemoryGlobalLaptopPlanStore()
    {
        return new MemoryGlobalLaptopPlanStore(new Dictionary<string, GlobalLaptopPlan>(), new List<string>());
    }

    public static IGlobalLaptopPlanStore NewCachedGlobalLaptopPlanStore(MemoryGlobalLaptopPlanStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalLaptopPlanStore(inner, GlobalLaptopPlanCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<GlobalLaptopPlan> FindGlobalLaptopPlans(IGlobalLaptopPlanStore store, GlobalLaptopPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
