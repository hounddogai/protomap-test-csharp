namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan assignments by code.</summary>
public interface ISuccessionPlanAssignmentStore
{
    SuccessionPlanAssignment? Find(string code);
    void Save(SuccessionPlanAssignment item);
    bool Remove(string code);
    List<SuccessionPlanAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanAssignmentStore(Dictionary<string, SuccessionPlanAssignment> items, List<string> order) : ISuccessionPlanAssignmentStore
{
    public Dictionary<string, SuccessionPlanAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanAssignment item)
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

    public List<SuccessionPlanAssignment> List()
    {
        var outValue = new List<SuccessionPlanAssignment>();
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
public sealed class CachedSuccessionPlanAssignmentStore(MemorySuccessionPlanAssignmentStore inner, SuccessionPlanAssignmentCache cache, int hits, int misses) : ISuccessionPlanAssignmentStore
{
    public MemorySuccessionPlanAssignmentStore Inner { get; set; } = inner;
    public SuccessionPlanAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanAssignment? Find(string code)
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

    public void Save(SuccessionPlanAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan assignments. Empty fields match all.</summary>
public sealed record SuccessionPlanAssignmentQuery(string Label, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanAssignment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanAssignment> Apply(List<SuccessionPlanAssignment> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan assignment store.</summary>
public static class SuccessionPlanAssignmentStore
{
    public static ISuccessionPlanAssignmentStore NewMemorySuccessionPlanAssignmentStore()
    {
        return new MemorySuccessionPlanAssignmentStore(
            new Dictionary<string, SuccessionPlanAssignment>(),
            new List<string>());
    }

    public static ISuccessionPlanAssignmentStore NewCachedSuccessionPlanAssignmentStore(MemorySuccessionPlanAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanAssignmentStore(
            inner,
            SuccessionPlanAssignmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SuccessionPlanAssignment> FindSuccessionPlanAssignments(ISuccessionPlanAssignmentStore store, SuccessionPlanAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
