namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan estimates by code.</summary>
public interface IReturnPlanEstimateStore
{
    ReturnPlanEstimate? Find(string code);
    void Save(ReturnPlanEstimate item);
    bool Remove(string code);
    List<ReturnPlanEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanEstimateStore(Dictionary<string, ReturnPlanEstimate> items, List<string> order) : IReturnPlanEstimateStore
{
    public Dictionary<string, ReturnPlanEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanEstimate item)
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

    public List<ReturnPlanEstimate> List()
    {
        var outValue = new List<ReturnPlanEstimate>();
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
public sealed class CachedReturnPlanEstimateStore(MemoryReturnPlanEstimateStore inner, ReturnPlanEstimateCache cache, int hits, int misses) : IReturnPlanEstimateStore
{
    public MemoryReturnPlanEstimateStore Inner { get; set; } = inner;
    public ReturnPlanEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanEstimate? Find(string code)
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

    public void Save(ReturnPlanEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan estimates. Empty fields match all.</summary>
public sealed record ReturnPlanEstimateQuery(string Unit, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanEstimate item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReturnPlanEstimate> Apply(List<ReturnPlanEstimate> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan estimate store.</summary>
public static class ReturnPlanEstimateStore
{
    public static IReturnPlanEstimateStore NewMemoryReturnPlanEstimateStore()
    {
        return new MemoryReturnPlanEstimateStore(new Dictionary<string, ReturnPlanEstimate>(), new List<string>());
    }

    public static IReturnPlanEstimateStore NewCachedReturnPlanEstimateStore(MemoryReturnPlanEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanEstimateStore(inner, ReturnPlanEstimateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanEstimate> FindReturnPlanEstimates(IReturnPlanEstimateStore store, ReturnPlanEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
