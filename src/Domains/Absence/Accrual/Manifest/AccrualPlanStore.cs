namespace Acme.Workforce.Absence.Accrual.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual plans by code.</summary>
public interface IAccrualPlanStore
{
    AccrualPlan? Find(string code);
    void Save(AccrualPlan item);
    bool Remove(string code);
    List<AccrualPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualPlanStore(Dictionary<string, AccrualPlan> items, List<string> order) : IAccrualPlanStore
{
    public Dictionary<string, AccrualPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualPlan item)
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

    public List<AccrualPlan> List()
    {
        var outValue = new List<AccrualPlan>();
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
public sealed class CachedAccrualPlanStore(MemoryAccrualPlanStore inner, AccrualPlanCache cache, int hits, int misses) : IAccrualPlanStore
{
    public MemoryAccrualPlanStore Inner { get; set; } = inner;
    public AccrualPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualPlan? Find(string code)
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

    public void Save(AccrualPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual plans. Empty fields match all.</summary>
public sealed record AccrualPlanQuery(string Title, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualPlan item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<AccrualPlan> Apply(List<AccrualPlan> items)
    {
        var matched = Collections.Filter(items, (AccrualPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual plan store.</summary>
public static class AccrualPlanStore
{
    public static IAccrualPlanStore NewMemoryAccrualPlanStore()
    {
        return new MemoryAccrualPlanStore(new Dictionary<string, AccrualPlan>(), new List<string>());
    }

    public static IAccrualPlanStore NewCachedAccrualPlanStore(MemoryAccrualPlanStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualPlanStore(inner, AccrualPlanCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AccrualPlan> FindAccrualPlans(IAccrualPlanStore store, AccrualPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
