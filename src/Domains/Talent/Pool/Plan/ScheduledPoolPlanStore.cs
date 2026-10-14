namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled pool plans by code.</summary>
public interface IScheduledPoolPlanStore
{
    ScheduledPoolPlan? Find(string code);
    void Save(ScheduledPoolPlan item);
    bool Remove(string code);
    List<ScheduledPoolPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledPoolPlanStore(Dictionary<string, ScheduledPoolPlan> items, List<string> order) : IScheduledPoolPlanStore
{
    public Dictionary<string, ScheduledPoolPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledPoolPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledPoolPlan item)
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

    public List<ScheduledPoolPlan> List()
    {
        var outValue = new List<ScheduledPoolPlan>();
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
public sealed class CachedScheduledPoolPlanStore(MemoryScheduledPoolPlanStore inner, ScheduledPoolPlanCache cache, int hits, int misses) : IScheduledPoolPlanStore
{
    public MemoryScheduledPoolPlanStore Inner { get; set; } = inner;
    public ScheduledPoolPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledPoolPlan? Find(string code)
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

    public void Save(ScheduledPoolPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledPoolPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled pool plans. Empty fields match all.</summary>
public sealed record ScheduledPoolPlanQuery(string Name, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledPoolPlan item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<ScheduledPoolPlan> Apply(List<ScheduledPoolPlan> items)
    {
        var matched = Collections.Filter(items, (ScheduledPoolPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled pool plan store.</summary>
public static class ScheduledPoolPlanStore
{
    public static IScheduledPoolPlanStore NewMemoryScheduledPoolPlanStore()
    {
        return new MemoryScheduledPoolPlanStore(new Dictionary<string, ScheduledPoolPlan>(), new List<string>());
    }

    public static IScheduledPoolPlanStore NewCachedScheduledPoolPlanStore(MemoryScheduledPoolPlanStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledPoolPlanStore(inner, ScheduledPoolPlanCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ScheduledPoolPlan> FindScheduledPoolPlans(IScheduledPoolPlanStore store, ScheduledPoolPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
