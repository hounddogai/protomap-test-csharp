namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending asset return plans by code.</summary>
public interface IPendingAssetReturnPlanStore
{
    PendingAssetReturnPlan? Find(string code);
    void Save(PendingAssetReturnPlan item);
    bool Remove(string code);
    List<PendingAssetReturnPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingAssetReturnPlanStore(Dictionary<string, PendingAssetReturnPlan> items, List<string> order) : IPendingAssetReturnPlanStore
{
    public Dictionary<string, PendingAssetReturnPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingAssetReturnPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingAssetReturnPlan item)
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

    public List<PendingAssetReturnPlan> List()
    {
        var outValue = new List<PendingAssetReturnPlan>();
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
public sealed class CachedPendingAssetReturnPlanStore(MemoryPendingAssetReturnPlanStore inner, PendingAssetReturnPlanCache cache, int hits, int misses) : IPendingAssetReturnPlanStore
{
    public MemoryPendingAssetReturnPlanStore Inner { get; set; } = inner;
    public PendingAssetReturnPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingAssetReturnPlan? Find(string code)
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

    public void Save(PendingAssetReturnPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingAssetReturnPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending asset return plans. Empty fields match all.</summary>
public sealed record PendingAssetReturnPlanQuery(string Region, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PendingAssetReturnPlan item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<PendingAssetReturnPlan> Apply(List<PendingAssetReturnPlan> items)
    {
        var matched = Collections.Filter(items, (PendingAssetReturnPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending asset return plan store.</summary>
public static class PendingAssetReturnPlanStore
{
    public static IPendingAssetReturnPlanStore NewMemoryPendingAssetReturnPlanStore()
    {
        return new MemoryPendingAssetReturnPlanStore(
            new Dictionary<string, PendingAssetReturnPlan>(),
            new List<string>());
    }

    public static IPendingAssetReturnPlanStore NewCachedPendingAssetReturnPlanStore(MemoryPendingAssetReturnPlanStore inner, int ttl, IClock clock)
    {
        return new CachedPendingAssetReturnPlanStore(
            inner,
            PendingAssetReturnPlanCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PendingAssetReturnPlan> FindPendingAssetReturnPlans(IPendingAssetReturnPlanStore store, PendingAssetReturnPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
