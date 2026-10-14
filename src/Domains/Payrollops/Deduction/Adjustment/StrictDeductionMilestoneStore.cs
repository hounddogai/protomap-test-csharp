namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict deduction milestones by code.</summary>
public interface IStrictDeductionMilestoneStore
{
    StrictDeductionMilestone? Find(string code);
    void Save(StrictDeductionMilestone item);
    bool Remove(string code);
    List<StrictDeductionMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictDeductionMilestoneStore(Dictionary<string, StrictDeductionMilestone> items, List<string> order) : IStrictDeductionMilestoneStore
{
    public Dictionary<string, StrictDeductionMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictDeductionMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictDeductionMilestone item)
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

    public List<StrictDeductionMilestone> List()
    {
        var outValue = new List<StrictDeductionMilestone>();
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
public sealed class CachedStrictDeductionMilestoneStore(MemoryStrictDeductionMilestoneStore inner, StrictDeductionMilestoneCache cache, int hits, int misses) : IStrictDeductionMilestoneStore
{
    public MemoryStrictDeductionMilestoneStore Inner { get; set; } = inner;
    public StrictDeductionMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictDeductionMilestone? Find(string code)
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

    public void Save(StrictDeductionMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictDeductionMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict deduction milestones. Empty fields match all.</summary>
public sealed record StrictDeductionMilestoneQuery(string Name, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(StrictDeductionMilestone item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<StrictDeductionMilestone> Apply(List<StrictDeductionMilestone> items)
    {
        var matched = Collections.Filter(items, (StrictDeductionMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict deduction milestone store.</summary>
public static class StrictDeductionMilestoneStore
{
    public static IStrictDeductionMilestoneStore NewMemoryStrictDeductionMilestoneStore()
    {
        return new MemoryStrictDeductionMilestoneStore(
            new Dictionary<string, StrictDeductionMilestone>(),
            new List<string>());
    }

    public static IStrictDeductionMilestoneStore NewCachedStrictDeductionMilestoneStore(MemoryStrictDeductionMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedStrictDeductionMilestoneStore(
            inner,
            StrictDeductionMilestoneCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<StrictDeductionMilestone> FindStrictDeductionMilestones(IStrictDeductionMilestoneStore store, StrictDeductionMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
