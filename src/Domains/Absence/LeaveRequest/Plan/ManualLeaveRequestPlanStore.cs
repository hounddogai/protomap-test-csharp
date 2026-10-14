namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual leave request plans by code.</summary>
public interface IManualLeaveRequestPlanStore
{
    ManualLeaveRequestPlan? Find(string code);
    void Save(ManualLeaveRequestPlan item);
    bool Remove(string code);
    List<ManualLeaveRequestPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualLeaveRequestPlanStore(Dictionary<string, ManualLeaveRequestPlan> items, List<string> order) : IManualLeaveRequestPlanStore
{
    public Dictionary<string, ManualLeaveRequestPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualLeaveRequestPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualLeaveRequestPlan item)
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

    public List<ManualLeaveRequestPlan> List()
    {
        var outValue = new List<ManualLeaveRequestPlan>();
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
public sealed class CachedManualLeaveRequestPlanStore(MemoryManualLeaveRequestPlanStore inner, ManualLeaveRequestPlanCache cache, int hits, int misses) : IManualLeaveRequestPlanStore
{
    public MemoryManualLeaveRequestPlanStore Inner { get; set; } = inner;
    public ManualLeaveRequestPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualLeaveRequestPlan? Find(string code)
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

    public void Save(ManualLeaveRequestPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualLeaveRequestPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual leave request plans. Empty fields match all.</summary>
public sealed record ManualLeaveRequestPlanQuery(string Locale, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ManualLeaveRequestPlan item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<ManualLeaveRequestPlan> Apply(List<ManualLeaveRequestPlan> items)
    {
        var matched = Collections.Filter(items, (ManualLeaveRequestPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual leave request plan store.</summary>
public static class ManualLeaveRequestPlanStore
{
    public static IManualLeaveRequestPlanStore NewMemoryManualLeaveRequestPlanStore()
    {
        return new MemoryManualLeaveRequestPlanStore(
            new Dictionary<string, ManualLeaveRequestPlan>(),
            new List<string>());
    }

    public static IManualLeaveRequestPlanStore NewCachedManualLeaveRequestPlanStore(MemoryManualLeaveRequestPlanStore inner, int ttl, IClock clock)
    {
        return new CachedManualLeaveRequestPlanStore(
            inner,
            ManualLeaveRequestPlanCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ManualLeaveRequestPlan> FindManualLeaveRequestPlans(IManualLeaveRequestPlanStore store, ManualLeaveRequestPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
