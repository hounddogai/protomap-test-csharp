namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback timesheet plans by code.</summary>
public interface IFallbackTimesheetPlanStore
{
    FallbackTimesheetPlan? Find(string code);
    void Save(FallbackTimesheetPlan item);
    bool Remove(string code);
    List<FallbackTimesheetPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackTimesheetPlanStore(Dictionary<string, FallbackTimesheetPlan> items, List<string> order) : IFallbackTimesheetPlanStore
{
    public Dictionary<string, FallbackTimesheetPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackTimesheetPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackTimesheetPlan item)
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

    public List<FallbackTimesheetPlan> List()
    {
        var outValue = new List<FallbackTimesheetPlan>();
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
public sealed class CachedFallbackTimesheetPlanStore(MemoryFallbackTimesheetPlanStore inner, FallbackTimesheetPlanCache cache, int hits, int misses) : IFallbackTimesheetPlanStore
{
    public MemoryFallbackTimesheetPlanStore Inner { get; set; } = inner;
    public FallbackTimesheetPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackTimesheetPlan? Find(string code)
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

    public void Save(FallbackTimesheetPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackTimesheetPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback timesheet plans. Empty fields match all.</summary>
public sealed record FallbackTimesheetPlanQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackTimesheetPlan item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<FallbackTimesheetPlan> Apply(List<FallbackTimesheetPlan> items)
    {
        var matched = Collections.Filter(items, (FallbackTimesheetPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback timesheet plan store.</summary>
public static class FallbackTimesheetPlanStore
{
    public static IFallbackTimesheetPlanStore NewMemoryFallbackTimesheetPlanStore()
    {
        return new MemoryFallbackTimesheetPlanStore(
            new Dictionary<string, FallbackTimesheetPlan>(),
            new List<string>());
    }

    public static IFallbackTimesheetPlanStore NewCachedFallbackTimesheetPlanStore(MemoryFallbackTimesheetPlanStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackTimesheetPlanStore(inner, FallbackTimesheetPlanCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<FallbackTimesheetPlan> FindFallbackTimesheetPlans(IFallbackTimesheetPlanStore store, FallbackTimesheetPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
