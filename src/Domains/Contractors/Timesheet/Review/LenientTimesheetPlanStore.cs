namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient timesheet plans by code.</summary>
public interface ILenientTimesheetPlanStore
{
    LenientTimesheetPlan? Find(string code);
    void Save(LenientTimesheetPlan item);
    bool Remove(string code);
    List<LenientTimesheetPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientTimesheetPlanStore(Dictionary<string, LenientTimesheetPlan> items, List<string> order) : ILenientTimesheetPlanStore
{
    public Dictionary<string, LenientTimesheetPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientTimesheetPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientTimesheetPlan item)
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

    public List<LenientTimesheetPlan> List()
    {
        var outValue = new List<LenientTimesheetPlan>();
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
public sealed class CachedLenientTimesheetPlanStore(MemoryLenientTimesheetPlanStore inner, LenientTimesheetPlanCache cache, int hits, int misses) : ILenientTimesheetPlanStore
{
    public MemoryLenientTimesheetPlanStore Inner { get; set; } = inner;
    public LenientTimesheetPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientTimesheetPlan? Find(string code)
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

    public void Save(LenientTimesheetPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientTimesheetPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient timesheet plans. Empty fields match all.</summary>
public sealed record LenientTimesheetPlanQuery(string Segment, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(LenientTimesheetPlan item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientTimesheetPlan> Apply(List<LenientTimesheetPlan> items)
    {
        var matched = Collections.Filter(items, (LenientTimesheetPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient timesheet plan store.</summary>
public static class LenientTimesheetPlanStore
{
    public static ILenientTimesheetPlanStore NewMemoryLenientTimesheetPlanStore()
    {
        return new MemoryLenientTimesheetPlanStore(
            new Dictionary<string, LenientTimesheetPlan>(),
            new List<string>());
    }

    public static ILenientTimesheetPlanStore NewCachedLenientTimesheetPlanStore(MemoryLenientTimesheetPlanStore inner, int ttl, IClock clock)
    {
        return new CachedLenientTimesheetPlanStore(inner, LenientTimesheetPlanCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LenientTimesheetPlan> FindLenientTimesheetPlans(ILenientTimesheetPlanStore store, LenientTimesheetPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
