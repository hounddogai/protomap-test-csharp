namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling asset return plans by code.</summary>
public interface IRollingAssetReturnPlanStore
{
    RollingAssetReturnPlan? Find(string code);
    void Save(RollingAssetReturnPlan item);
    bool Remove(string code);
    List<RollingAssetReturnPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingAssetReturnPlanStore(Dictionary<string, RollingAssetReturnPlan> items, List<string> order) : IRollingAssetReturnPlanStore
{
    public Dictionary<string, RollingAssetReturnPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingAssetReturnPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingAssetReturnPlan item)
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

    public List<RollingAssetReturnPlan> List()
    {
        var outValue = new List<RollingAssetReturnPlan>();
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
public sealed class CachedRollingAssetReturnPlanStore(MemoryRollingAssetReturnPlanStore inner, RollingAssetReturnPlanCache cache, int hits, int misses) : IRollingAssetReturnPlanStore
{
    public MemoryRollingAssetReturnPlanStore Inner { get; set; } = inner;
    public RollingAssetReturnPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingAssetReturnPlan? Find(string code)
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

    public void Save(RollingAssetReturnPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingAssetReturnPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling asset return plans. Empty fields match all.</summary>
public sealed record RollingAssetReturnPlanQuery(string Unit, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(RollingAssetReturnPlan item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingAssetReturnPlan> Apply(List<RollingAssetReturnPlan> items)
    {
        var matched = Collections.Filter(items, (RollingAssetReturnPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling asset return plan store.</summary>
public static class RollingAssetReturnPlanStore
{
    public static IRollingAssetReturnPlanStore NewMemoryRollingAssetReturnPlanStore()
    {
        return new MemoryRollingAssetReturnPlanStore(
            new Dictionary<string, RollingAssetReturnPlan>(),
            new List<string>());
    }

    public static IRollingAssetReturnPlanStore NewCachedRollingAssetReturnPlanStore(MemoryRollingAssetReturnPlanStore inner, int ttl, IClock clock)
    {
        return new CachedRollingAssetReturnPlanStore(
            inner,
            RollingAssetReturnPlanCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RollingAssetReturnPlan> FindRollingAssetReturnPlans(IRollingAssetReturnPlanStore store, RollingAssetReturnPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
