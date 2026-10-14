namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan cycles by code.</summary>
public interface IReturnPlanCycleStore
{
    ReturnPlanCycle? Find(string code);
    void Save(ReturnPlanCycle item);
    bool Remove(string code);
    List<ReturnPlanCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanCycleStore(Dictionary<string, ReturnPlanCycle> items, List<string> order) : IReturnPlanCycleStore
{
    public Dictionary<string, ReturnPlanCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanCycle item)
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

    public List<ReturnPlanCycle> List()
    {
        var outValue = new List<ReturnPlanCycle>();
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
public sealed class CachedReturnPlanCycleStore(MemoryReturnPlanCycleStore inner, ReturnPlanCycleCache cache, int hits, int misses) : IReturnPlanCycleStore
{
    public MemoryReturnPlanCycleStore Inner { get; set; } = inner;
    public ReturnPlanCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanCycle? Find(string code)
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

    public void Save(ReturnPlanCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan cycles. Empty fields match all.</summary>
public sealed record ReturnPlanCycleQuery(string Unit, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanCycle item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReturnPlanCycle> Apply(List<ReturnPlanCycle> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan cycle store.</summary>
public static class ReturnPlanCycleStore
{
    public static IReturnPlanCycleStore NewMemoryReturnPlanCycleStore()
    {
        return new MemoryReturnPlanCycleStore(new Dictionary<string, ReturnPlanCycle>(), new List<string>());
    }

    public static IReturnPlanCycleStore NewCachedReturnPlanCycleStore(MemoryReturnPlanCycleStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanCycleStore(inner, ReturnPlanCycleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanCycle> FindReturnPlanCycles(IReturnPlanCycleStore store, ReturnPlanCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
