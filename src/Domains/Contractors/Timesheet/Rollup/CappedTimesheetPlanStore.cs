namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped timesheet plans by code.</summary>
public interface ICappedTimesheetPlanStore
{
    CappedTimesheetPlan? Find(string code);
    void Save(CappedTimesheetPlan item);
    bool Remove(string code);
    List<CappedTimesheetPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedTimesheetPlanStore(Dictionary<string, CappedTimesheetPlan> items, List<string> order) : ICappedTimesheetPlanStore
{
    public Dictionary<string, CappedTimesheetPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedTimesheetPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedTimesheetPlan item)
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

    public List<CappedTimesheetPlan> List()
    {
        var outValue = new List<CappedTimesheetPlan>();
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
public sealed class CachedCappedTimesheetPlanStore(MemoryCappedTimesheetPlanStore inner, CappedTimesheetPlanCache cache, int hits, int misses) : ICappedTimesheetPlanStore
{
    public MemoryCappedTimesheetPlanStore Inner { get; set; } = inner;
    public CappedTimesheetPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedTimesheetPlan? Find(string code)
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

    public void Save(CappedTimesheetPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedTimesheetPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped timesheet plans. Empty fields match all.</summary>
public sealed record CappedTimesheetPlanQuery(string Name, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(CappedTimesheetPlan item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedTimesheetPlan> Apply(List<CappedTimesheetPlan> items)
    {
        var matched = Collections.Filter(items, (CappedTimesheetPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped timesheet plan store.</summary>
public static class CappedTimesheetPlanStore
{
    public static ICappedTimesheetPlanStore NewMemoryCappedTimesheetPlanStore()
    {
        return new MemoryCappedTimesheetPlanStore(new Dictionary<string, CappedTimesheetPlan>(), new List<string>());
    }

    public static ICappedTimesheetPlanStore NewCachedCappedTimesheetPlanStore(MemoryCappedTimesheetPlanStore inner, int ttl, IClock clock)
    {
        return new CachedCappedTimesheetPlanStore(inner, CappedTimesheetPlanCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CappedTimesheetPlan> FindCappedTimesheetPlans(ICappedTimesheetPlanStore store, CappedTimesheetPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
