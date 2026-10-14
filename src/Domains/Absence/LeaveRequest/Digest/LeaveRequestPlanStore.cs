namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request plans by code.</summary>
public interface ILeaveRequestPlanStore
{
    LeaveRequestPlan? Find(string code);
    void Save(LeaveRequestPlan item);
    bool Remove(string code);
    List<LeaveRequestPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestPlanStore(Dictionary<string, LeaveRequestPlan> items, List<string> order) : ILeaveRequestPlanStore
{
    public Dictionary<string, LeaveRequestPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestPlan item)
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

    public List<LeaveRequestPlan> List()
    {
        var outValue = new List<LeaveRequestPlan>();
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
public sealed class CachedLeaveRequestPlanStore(MemoryLeaveRequestPlanStore inner, LeaveRequestPlanCache cache, int hits, int misses) : ILeaveRequestPlanStore
{
    public MemoryLeaveRequestPlanStore Inner { get; set; } = inner;
    public LeaveRequestPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestPlan? Find(string code)
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

    public void Save(LeaveRequestPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request plans. Empty fields match all.</summary>
public sealed record LeaveRequestPlanQuery(string Name, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestPlan item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestPlan> Apply(List<LeaveRequestPlan> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request plan store.</summary>
public static class LeaveRequestPlanStore
{
    public static ILeaveRequestPlanStore NewMemoryLeaveRequestPlanStore()
    {
        return new MemoryLeaveRequestPlanStore(new Dictionary<string, LeaveRequestPlan>(), new List<string>());
    }

    public static ILeaveRequestPlanStore NewCachedLeaveRequestPlanStore(MemoryLeaveRequestPlanStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestPlanStore(inner, LeaveRequestPlanCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestPlan> FindLeaveRequestPlans(ILeaveRequestPlanStore store, LeaveRequestPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
