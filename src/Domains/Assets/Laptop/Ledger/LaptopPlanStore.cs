namespace Acme.Workforce.Assets.Laptop.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop plans by code.</summary>
public interface ILaptopPlanStore
{
    LaptopPlan? Find(string code);
    void Save(LaptopPlan item);
    bool Remove(string code);
    List<LaptopPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopPlanStore(Dictionary<string, LaptopPlan> items, List<string> order) : ILaptopPlanStore
{
    public Dictionary<string, LaptopPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopPlan item)
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

    public List<LaptopPlan> List()
    {
        var outValue = new List<LaptopPlan>();
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
public sealed class CachedLaptopPlanStore(MemoryLaptopPlanStore inner, LaptopPlanCache cache, int hits, int misses) : ILaptopPlanStore
{
    public MemoryLaptopPlanStore Inner { get; set; } = inner;
    public LaptopPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopPlan? Find(string code)
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

    public void Save(LaptopPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop plans. Empty fields match all.</summary>
public sealed record LaptopPlanQuery(string Country, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopPlan item)
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

    public List<LaptopPlan> Apply(List<LaptopPlan> items)
    {
        var matched = Collections.Filter(items, (LaptopPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop plan store.</summary>
public static class LaptopPlanStore
{
    public static ILaptopPlanStore NewMemoryLaptopPlanStore()
    {
        return new MemoryLaptopPlanStore(new Dictionary<string, LaptopPlan>(), new List<string>());
    }

    public static ILaptopPlanStore NewCachedLaptopPlanStore(MemoryLaptopPlanStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopPlanStore(inner, LaptopPlanCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LaptopPlan> FindLaptopPlans(ILaptopPlanStore store, LaptopPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
