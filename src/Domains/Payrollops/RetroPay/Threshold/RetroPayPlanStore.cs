namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay plans by code.</summary>
public interface IRetroPayPlanStore
{
    RetroPayPlan? Find(string code);
    void Save(RetroPayPlan item);
    bool Remove(string code);
    List<RetroPayPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayPlanStore(Dictionary<string, RetroPayPlan> items, List<string> order) : IRetroPayPlanStore
{
    public Dictionary<string, RetroPayPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayPlan item)
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

    public List<RetroPayPlan> List()
    {
        var outValue = new List<RetroPayPlan>();
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
public sealed class CachedRetroPayPlanStore(MemoryRetroPayPlanStore inner, RetroPayPlanCache cache, int hits, int misses) : IRetroPayPlanStore
{
    public MemoryRetroPayPlanStore Inner { get; set; } = inner;
    public RetroPayPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayPlan? Find(string code)
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

    public void Save(RetroPayPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay plans. Empty fields match all.</summary>
public sealed record RetroPayPlanQuery(string Team, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayPlan item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<RetroPayPlan> Apply(List<RetroPayPlan> items)
    {
        var matched = Collections.Filter(items, (RetroPayPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay plan store.</summary>
public static class RetroPayPlanStore
{
    public static IRetroPayPlanStore NewMemoryRetroPayPlanStore()
    {
        return new MemoryRetroPayPlanStore(new Dictionary<string, RetroPayPlan>(), new List<string>());
    }

    public static IRetroPayPlanStore NewCachedRetroPayPlanStore(MemoryRetroPayPlanStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayPlanStore(inner, RetroPayPlanCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RetroPayPlan> FindRetroPayPlans(IRetroPayPlanStore store, RetroPayPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
