namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary accrual plans by code.</summary>
public interface IPrimaryAccrualPlanStore
{
    PrimaryAccrualPlan? Find(string code);
    void Save(PrimaryAccrualPlan item);
    bool Remove(string code);
    List<PrimaryAccrualPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAccrualPlanStore(Dictionary<string, PrimaryAccrualPlan> items, List<string> order) : IPrimaryAccrualPlanStore
{
    public Dictionary<string, PrimaryAccrualPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAccrualPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAccrualPlan item)
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

    public List<PrimaryAccrualPlan> List()
    {
        var outValue = new List<PrimaryAccrualPlan>();
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
public sealed class CachedPrimaryAccrualPlanStore(MemoryPrimaryAccrualPlanStore inner, PrimaryAccrualPlanCache cache, int hits, int misses) : IPrimaryAccrualPlanStore
{
    public MemoryPrimaryAccrualPlanStore Inner { get; set; } = inner;
    public PrimaryAccrualPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAccrualPlan? Find(string code)
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

    public void Save(PrimaryAccrualPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAccrualPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary accrual plans. Empty fields match all.</summary>
public sealed record PrimaryAccrualPlanQuery(string Country, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAccrualPlan item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryAccrualPlan> Apply(List<PrimaryAccrualPlan> items)
    {
        var matched = Collections.Filter(items, (PrimaryAccrualPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary accrual plan store.</summary>
public static class PrimaryAccrualPlanStore
{
    public static IPrimaryAccrualPlanStore NewMemoryPrimaryAccrualPlanStore()
    {
        return new MemoryPrimaryAccrualPlanStore(new Dictionary<string, PrimaryAccrualPlan>(), new List<string>());
    }

    public static IPrimaryAccrualPlanStore NewCachedPrimaryAccrualPlanStore(MemoryPrimaryAccrualPlanStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAccrualPlanStore(inner, PrimaryAccrualPlanCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PrimaryAccrualPlan> FindPrimaryAccrualPlans(IPrimaryAccrualPlanStore store, PrimaryAccrualPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
