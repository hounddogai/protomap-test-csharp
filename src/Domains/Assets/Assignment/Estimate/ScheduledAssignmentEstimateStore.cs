namespace Acme.Workforce.Assets.Assignment.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled assignment estimates by code.</summary>
public interface IScheduledAssignmentEstimateStore
{
    ScheduledAssignmentEstimate? Find(string code);
    void Save(ScheduledAssignmentEstimate item);
    bool Remove(string code);
    List<ScheduledAssignmentEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledAssignmentEstimateStore(Dictionary<string, ScheduledAssignmentEstimate> items, List<string> order) : IScheduledAssignmentEstimateStore
{
    public Dictionary<string, ScheduledAssignmentEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledAssignmentEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledAssignmentEstimate item)
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

    public List<ScheduledAssignmentEstimate> List()
    {
        var outValue = new List<ScheduledAssignmentEstimate>();
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
public sealed class CachedScheduledAssignmentEstimateStore(MemoryScheduledAssignmentEstimateStore inner, ScheduledAssignmentEstimateCache cache, int hits, int misses) : IScheduledAssignmentEstimateStore
{
    public MemoryScheduledAssignmentEstimateStore Inner { get; set; } = inner;
    public ScheduledAssignmentEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledAssignmentEstimate? Find(string code)
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

    public void Save(ScheduledAssignmentEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledAssignmentEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled assignment estimates. Empty fields match all.</summary>
public sealed record ScheduledAssignmentEstimateQuery(string Label, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledAssignmentEstimate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledAssignmentEstimate> Apply(List<ScheduledAssignmentEstimate> items)
    {
        var matched = Collections.Filter(items, (ScheduledAssignmentEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled assignment estimate store.</summary>
public static class ScheduledAssignmentEstimateStore
{
    public static IScheduledAssignmentEstimateStore NewMemoryScheduledAssignmentEstimateStore()
    {
        return new MemoryScheduledAssignmentEstimateStore(
            new Dictionary<string, ScheduledAssignmentEstimate>(),
            new List<string>());
    }

    public static IScheduledAssignmentEstimateStore NewCachedScheduledAssignmentEstimateStore(MemoryScheduledAssignmentEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledAssignmentEstimateStore(
            inner,
            ScheduledAssignmentEstimateCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledAssignmentEstimate> FindScheduledAssignmentEstimates(IScheduledAssignmentEstimateStore store, ScheduledAssignmentEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
