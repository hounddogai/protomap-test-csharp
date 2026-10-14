namespace Acme.Workforce.Compensation.EquityGrant.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled equity grant plans by code.</summary>
public interface IScheduledEquityGrantPlanStore
{
    ScheduledEquityGrantPlan? Find(string code);
    void Save(ScheduledEquityGrantPlan item);
    bool Remove(string code);
    List<ScheduledEquityGrantPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledEquityGrantPlanStore(Dictionary<string, ScheduledEquityGrantPlan> items, List<string> order) : IScheduledEquityGrantPlanStore
{
    public Dictionary<string, ScheduledEquityGrantPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledEquityGrantPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledEquityGrantPlan item)
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

    public List<ScheduledEquityGrantPlan> List()
    {
        var outValue = new List<ScheduledEquityGrantPlan>();
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
public sealed class CachedScheduledEquityGrantPlanStore(MemoryScheduledEquityGrantPlanStore inner, ScheduledEquityGrantPlanCache cache, int hits, int misses) : IScheduledEquityGrantPlanStore
{
    public MemoryScheduledEquityGrantPlanStore Inner { get; set; } = inner;
    public ScheduledEquityGrantPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledEquityGrantPlan? Find(string code)
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

    public void Save(ScheduledEquityGrantPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledEquityGrantPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled equity grant plans. Empty fields match all.</summary>
public sealed record ScheduledEquityGrantPlanQuery(string Department, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledEquityGrantPlan item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledEquityGrantPlan> Apply(List<ScheduledEquityGrantPlan> items)
    {
        var matched = Collections.Filter(items, (ScheduledEquityGrantPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled equity grant plan store.</summary>
public static class ScheduledEquityGrantPlanStore
{
    public static IScheduledEquityGrantPlanStore NewMemoryScheduledEquityGrantPlanStore()
    {
        return new MemoryScheduledEquityGrantPlanStore(
            new Dictionary<string, ScheduledEquityGrantPlan>(),
            new List<string>());
    }

    public static IScheduledEquityGrantPlanStore NewCachedScheduledEquityGrantPlanStore(MemoryScheduledEquityGrantPlanStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledEquityGrantPlanStore(
            inner,
            ScheduledEquityGrantPlanCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledEquityGrantPlan> FindScheduledEquityGrantPlans(IScheduledEquityGrantPlanStore store, ScheduledEquityGrantPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
