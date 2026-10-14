namespace Acme.Workforce.Assets.Assignment.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment estimates by code.</summary>
public interface IAssignmentEstimateStore
{
    AssignmentEstimate? Find(string code);
    void Save(AssignmentEstimate item);
    bool Remove(string code);
    List<AssignmentEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentEstimateStore(Dictionary<string, AssignmentEstimate> items, List<string> order) : IAssignmentEstimateStore
{
    public Dictionary<string, AssignmentEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentEstimate item)
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

    public List<AssignmentEstimate> List()
    {
        var outValue = new List<AssignmentEstimate>();
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
public sealed class CachedAssignmentEstimateStore(MemoryAssignmentEstimateStore inner, AssignmentEstimateCache cache, int hits, int misses) : IAssignmentEstimateStore
{
    public MemoryAssignmentEstimateStore Inner { get; set; } = inner;
    public AssignmentEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentEstimate? Find(string code)
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

    public void Save(AssignmentEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment estimates. Empty fields match all.</summary>
public sealed record AssignmentEstimateQuery(string Name, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentEstimate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentEstimate> Apply(List<AssignmentEstimate> items)
    {
        var matched = Collections.Filter(items, (AssignmentEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment estimate store.</summary>
public static class AssignmentEstimateStore
{
    public static IAssignmentEstimateStore NewMemoryAssignmentEstimateStore()
    {
        return new MemoryAssignmentEstimateStore(new Dictionary<string, AssignmentEstimate>(), new List<string>());
    }

    public static IAssignmentEstimateStore NewCachedAssignmentEstimateStore(MemoryAssignmentEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentEstimateStore(inner, AssignmentEstimateCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AssignmentEstimate> FindAssignmentEstimates(IAssignmentEstimateStore store, AssignmentEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
