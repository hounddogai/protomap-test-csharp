namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict pool plans by code.</summary>
public interface IStrictPoolPlanStore
{
    StrictPoolPlan? Find(string code);
    void Save(StrictPoolPlan item);
    bool Remove(string code);
    List<StrictPoolPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictPoolPlanStore(Dictionary<string, StrictPoolPlan> items, List<string> order) : IStrictPoolPlanStore
{
    public Dictionary<string, StrictPoolPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictPoolPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictPoolPlan item)
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

    public List<StrictPoolPlan> List()
    {
        var outValue = new List<StrictPoolPlan>();
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
public sealed class CachedStrictPoolPlanStore(MemoryStrictPoolPlanStore inner, StrictPoolPlanCache cache, int hits, int misses) : IStrictPoolPlanStore
{
    public MemoryStrictPoolPlanStore Inner { get; set; } = inner;
    public StrictPoolPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictPoolPlan? Find(string code)
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

    public void Save(StrictPoolPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictPoolPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict pool plans. Empty fields match all.</summary>
public sealed record StrictPoolPlanQuery(string Label, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(StrictPoolPlan item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictPoolPlan> Apply(List<StrictPoolPlan> items)
    {
        var matched = Collections.Filter(items, (StrictPoolPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict pool plan store.</summary>
public static class StrictPoolPlanStore
{
    public static IStrictPoolPlanStore NewMemoryStrictPoolPlanStore()
    {
        return new MemoryStrictPoolPlanStore(new Dictionary<string, StrictPoolPlan>(), new List<string>());
    }

    public static IStrictPoolPlanStore NewCachedStrictPoolPlanStore(MemoryStrictPoolPlanStore inner, int ttl, IClock clock)
    {
        return new CachedStrictPoolPlanStore(inner, StrictPoolPlanCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<StrictPoolPlan> FindStrictPoolPlans(IStrictPoolPlanStore store, StrictPoolPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
