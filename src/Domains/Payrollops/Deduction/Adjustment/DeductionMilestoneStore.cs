namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction milestones by code.</summary>
public interface IDeductionMilestoneStore
{
    DeductionMilestone? Find(string code);
    void Save(DeductionMilestone item);
    bool Remove(string code);
    List<DeductionMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionMilestoneStore(Dictionary<string, DeductionMilestone> items, List<string> order) : IDeductionMilestoneStore
{
    public Dictionary<string, DeductionMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionMilestone item)
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

    public List<DeductionMilestone> List()
    {
        var outValue = new List<DeductionMilestone>();
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
public sealed class CachedDeductionMilestoneStore(MemoryDeductionMilestoneStore inner, DeductionMilestoneCache cache, int hits, int misses) : IDeductionMilestoneStore
{
    public MemoryDeductionMilestoneStore Inner { get; set; } = inner;
    public DeductionMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionMilestone? Find(string code)
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

    public void Save(DeductionMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction milestones. Empty fields match all.</summary>
public sealed record DeductionMilestoneQuery(string Name, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionMilestone item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionMilestone> Apply(List<DeductionMilestone> items)
    {
        var matched = Collections.Filter(items, (DeductionMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction milestone store.</summary>
public static class DeductionMilestoneStore
{
    public static IDeductionMilestoneStore NewMemoryDeductionMilestoneStore()
    {
        return new MemoryDeductionMilestoneStore(new Dictionary<string, DeductionMilestone>(), new List<string>());
    }

    public static IDeductionMilestoneStore NewCachedDeductionMilestoneStore(MemoryDeductionMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionMilestoneStore(inner, DeductionMilestoneCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DeductionMilestone> FindDeductionMilestones(IDeductionMilestoneStore store, DeductionMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
