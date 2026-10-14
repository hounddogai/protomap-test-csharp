namespace Acme.Workforce.Assets.Assignment.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment plans by code.</summary>
public interface IAssignmentPlanStore
{
    AssignmentPlan? Find(string code);
    void Save(AssignmentPlan item);
    bool Remove(string code);
    List<AssignmentPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentPlanStore(Dictionary<string, AssignmentPlan> items, List<string> order) : IAssignmentPlanStore
{
    public Dictionary<string, AssignmentPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentPlan item)
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

    public List<AssignmentPlan> List()
    {
        var outValue = new List<AssignmentPlan>();
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
public sealed class CachedAssignmentPlanStore(MemoryAssignmentPlanStore inner, AssignmentPlanCache cache, int hits, int misses) : IAssignmentPlanStore
{
    public MemoryAssignmentPlanStore Inner { get; set; } = inner;
    public AssignmentPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentPlan? Find(string code)
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

    public void Save(AssignmentPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment plans. Empty fields match all.</summary>
public sealed record AssignmentPlanQuery(string Region, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentPlan item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<AssignmentPlan> Apply(List<AssignmentPlan> items)
    {
        var matched = Collections.Filter(items, (AssignmentPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment plan store.</summary>
public static class AssignmentPlanStore
{
    public static IAssignmentPlanStore NewMemoryAssignmentPlanStore()
    {
        return new MemoryAssignmentPlanStore(new Dictionary<string, AssignmentPlan>(), new List<string>());
    }

    public static IAssignmentPlanStore NewCachedAssignmentPlanStore(MemoryAssignmentPlanStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentPlanStore(inner, AssignmentPlanCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AssignmentPlan> FindAssignmentPlans(IAssignmentPlanStore store, AssignmentPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
