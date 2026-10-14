namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped asset return plans by code.</summary>
public interface ICappedAssetReturnPlanStore
{
    CappedAssetReturnPlan? Find(string code);
    void Save(CappedAssetReturnPlan item);
    bool Remove(string code);
    List<CappedAssetReturnPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedAssetReturnPlanStore(Dictionary<string, CappedAssetReturnPlan> items, List<string> order) : ICappedAssetReturnPlanStore
{
    public Dictionary<string, CappedAssetReturnPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedAssetReturnPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedAssetReturnPlan item)
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

    public List<CappedAssetReturnPlan> List()
    {
        var outValue = new List<CappedAssetReturnPlan>();
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
public sealed class CachedCappedAssetReturnPlanStore(MemoryCappedAssetReturnPlanStore inner, CappedAssetReturnPlanCache cache, int hits, int misses) : ICappedAssetReturnPlanStore
{
    public MemoryCappedAssetReturnPlanStore Inner { get; set; } = inner;
    public CappedAssetReturnPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedAssetReturnPlan? Find(string code)
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

    public void Save(CappedAssetReturnPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedAssetReturnPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped asset return plans. Empty fields match all.</summary>
public sealed record CappedAssetReturnPlanQuery(string Segment, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(CappedAssetReturnPlan item)
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

    public List<CappedAssetReturnPlan> Apply(List<CappedAssetReturnPlan> items)
    {
        var matched = Collections.Filter(items, (CappedAssetReturnPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped asset return plan store.</summary>
public static class CappedAssetReturnPlanStore
{
    public static ICappedAssetReturnPlanStore NewMemoryCappedAssetReturnPlanStore()
    {
        return new MemoryCappedAssetReturnPlanStore(
            new Dictionary<string, CappedAssetReturnPlan>(),
            new List<string>());
    }

    public static ICappedAssetReturnPlanStore NewCachedCappedAssetReturnPlanStore(MemoryCappedAssetReturnPlanStore inner, int ttl, IClock clock)
    {
        return new CachedCappedAssetReturnPlanStore(inner, CappedAssetReturnPlanCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<CappedAssetReturnPlan> FindCappedAssetReturnPlans(ICappedAssetReturnPlanStore store, CappedAssetReturnPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
