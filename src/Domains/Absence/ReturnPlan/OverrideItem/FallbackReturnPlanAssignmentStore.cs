namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback return plan assignments by code.</summary>
public interface IFallbackReturnPlanAssignmentStore
{
    FallbackReturnPlanAssignment? Find(string code);
    void Save(FallbackReturnPlanAssignment item);
    bool Remove(string code);
    List<FallbackReturnPlanAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackReturnPlanAssignmentStore(Dictionary<string, FallbackReturnPlanAssignment> items, List<string> order) : IFallbackReturnPlanAssignmentStore
{
    public Dictionary<string, FallbackReturnPlanAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackReturnPlanAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackReturnPlanAssignment item)
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

    public List<FallbackReturnPlanAssignment> List()
    {
        var outValue = new List<FallbackReturnPlanAssignment>();
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
public sealed class CachedFallbackReturnPlanAssignmentStore(MemoryFallbackReturnPlanAssignmentStore inner, FallbackReturnPlanAssignmentCache cache, int hits, int misses) : IFallbackReturnPlanAssignmentStore
{
    public MemoryFallbackReturnPlanAssignmentStore Inner { get; set; } = inner;
    public FallbackReturnPlanAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackReturnPlanAssignment? Find(string code)
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

    public void Save(FallbackReturnPlanAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackReturnPlanAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback return plan assignments. Empty fields match all.</summary>
public sealed record FallbackReturnPlanAssignmentQuery(string Label, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackReturnPlanAssignment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackReturnPlanAssignment> Apply(List<FallbackReturnPlanAssignment> items)
    {
        var matched = Collections.Filter(items, (FallbackReturnPlanAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback return plan assignment store.</summary>
public static class FallbackReturnPlanAssignmentStore
{
    public static IFallbackReturnPlanAssignmentStore NewMemoryFallbackReturnPlanAssignmentStore()
    {
        return new MemoryFallbackReturnPlanAssignmentStore(
            new Dictionary<string, FallbackReturnPlanAssignment>(),
            new List<string>());
    }

    public static IFallbackReturnPlanAssignmentStore NewCachedFallbackReturnPlanAssignmentStore(MemoryFallbackReturnPlanAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackReturnPlanAssignmentStore(
            inner,
            FallbackReturnPlanAssignmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<FallbackReturnPlanAssignment> FindFallbackReturnPlanAssignments(IFallbackReturnPlanAssignmentStore store, FallbackReturnPlanAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
