namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run milestones by code.</summary>
public interface IPayRunMilestoneStore
{
    PayRunMilestone? Find(string code);
    void Save(PayRunMilestone item);
    bool Remove(string code);
    List<PayRunMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunMilestoneStore(Dictionary<string, PayRunMilestone> items, List<string> order) : IPayRunMilestoneStore
{
    public Dictionary<string, PayRunMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunMilestone item)
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

    public List<PayRunMilestone> List()
    {
        var outValue = new List<PayRunMilestone>();
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
public sealed class CachedPayRunMilestoneStore(MemoryPayRunMilestoneStore inner, PayRunMilestoneCache cache, int hits, int misses) : IPayRunMilestoneStore
{
    public MemoryPayRunMilestoneStore Inner { get; set; } = inner;
    public PayRunMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunMilestone? Find(string code)
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

    public void Save(PayRunMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run milestones. Empty fields match all.</summary>
public sealed record PayRunMilestoneQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunMilestone item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunMilestone> Apply(List<PayRunMilestone> items)
    {
        var matched = Collections.Filter(items, (PayRunMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run milestone store.</summary>
public static class PayRunMilestoneStore
{
    public static IPayRunMilestoneStore NewMemoryPayRunMilestoneStore()
    {
        return new MemoryPayRunMilestoneStore(new Dictionary<string, PayRunMilestone>(), new List<string>());
    }

    public static IPayRunMilestoneStore NewCachedPayRunMilestoneStore(MemoryPayRunMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunMilestoneStore(inner, PayRunMilestoneCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayRunMilestone> FindPayRunMilestones(IPayRunMilestoneStore store, PayRunMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
