namespace Acme.Workforce.Talent.Pool.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool plans by code.</summary>
public interface IPoolPlanStore
{
    PoolPlan? Find(string code);
    void Save(PoolPlan item);
    bool Remove(string code);
    List<PoolPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolPlanStore(Dictionary<string, PoolPlan> items, List<string> order) : IPoolPlanStore
{
    public Dictionary<string, PoolPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolPlan item)
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

    public List<PoolPlan> List()
    {
        var outValue = new List<PoolPlan>();
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
public sealed class CachedPoolPlanStore(MemoryPoolPlanStore inner, PoolPlanCache cache, int hits, int misses) : IPoolPlanStore
{
    public MemoryPoolPlanStore Inner { get; set; } = inner;
    public PoolPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolPlan? Find(string code)
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

    public void Save(PoolPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool plans. Empty fields match all.</summary>
public sealed record PoolPlanQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(PoolPlan item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolPlan> Apply(List<PoolPlan> items)
    {
        var matched = Collections.Filter(items, (PoolPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool plan store.</summary>
public static class PoolPlanStore
{
    public static IPoolPlanStore NewMemoryPoolPlanStore()
    {
        return new MemoryPoolPlanStore(new Dictionary<string, PoolPlan>(), new List<string>());
    }

    public static IPoolPlanStore NewCachedPoolPlanStore(MemoryPoolPlanStore inner, int ttl, IClock clock)
    {
        return new CachedPoolPlanStore(inner, PoolPlanCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PoolPlan> FindPoolPlans(IPoolPlanStore store, PoolPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
