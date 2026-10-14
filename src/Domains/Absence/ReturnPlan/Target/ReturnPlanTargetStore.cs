namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan targets by code.</summary>
public interface IReturnPlanTargetStore
{
    ReturnPlanTarget? Find(string code);
    void Save(ReturnPlanTarget item);
    bool Remove(string code);
    List<ReturnPlanTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanTargetStore(Dictionary<string, ReturnPlanTarget> items, List<string> order) : IReturnPlanTargetStore
{
    public Dictionary<string, ReturnPlanTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanTarget item)
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

    public List<ReturnPlanTarget> List()
    {
        var outValue = new List<ReturnPlanTarget>();
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
public sealed class CachedReturnPlanTargetStore(MemoryReturnPlanTargetStore inner, ReturnPlanTargetCache cache, int hits, int misses) : IReturnPlanTargetStore
{
    public MemoryReturnPlanTargetStore Inner { get; set; } = inner;
    public ReturnPlanTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanTarget? Find(string code)
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

    public void Save(ReturnPlanTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan targets. Empty fields match all.</summary>
public sealed record ReturnPlanTargetQuery(string Country, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanTarget item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReturnPlanTarget> Apply(List<ReturnPlanTarget> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan target store.</summary>
public static class ReturnPlanTargetStore
{
    public static IReturnPlanTargetStore NewMemoryReturnPlanTargetStore()
    {
        return new MemoryReturnPlanTargetStore(new Dictionary<string, ReturnPlanTarget>(), new List<string>());
    }

    public static IReturnPlanTargetStore NewCachedReturnPlanTargetStore(MemoryReturnPlanTargetStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanTargetStore(inner, ReturnPlanTargetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanTarget> FindReturnPlanTargets(IReturnPlanTargetStore store, ReturnPlanTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
