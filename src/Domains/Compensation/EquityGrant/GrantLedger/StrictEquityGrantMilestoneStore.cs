namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict equity grant milestones by code.</summary>
public interface IStrictEquityGrantMilestoneStore
{
    StrictEquityGrantMilestone? Find(string code);
    void Save(StrictEquityGrantMilestone item);
    bool Remove(string code);
    List<StrictEquityGrantMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictEquityGrantMilestoneStore(Dictionary<string, StrictEquityGrantMilestone> items, List<string> order) : IStrictEquityGrantMilestoneStore
{
    public Dictionary<string, StrictEquityGrantMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictEquityGrantMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictEquityGrantMilestone item)
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

    public List<StrictEquityGrantMilestone> List()
    {
        var outValue = new List<StrictEquityGrantMilestone>();
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
public sealed class CachedStrictEquityGrantMilestoneStore(MemoryStrictEquityGrantMilestoneStore inner, StrictEquityGrantMilestoneCache cache, int hits, int misses) : IStrictEquityGrantMilestoneStore
{
    public MemoryStrictEquityGrantMilestoneStore Inner { get; set; } = inner;
    public StrictEquityGrantMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictEquityGrantMilestone? Find(string code)
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

    public void Save(StrictEquityGrantMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictEquityGrantMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict equity grant milestones. Empty fields match all.</summary>
public sealed record StrictEquityGrantMilestoneQuery(string Region, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(StrictEquityGrantMilestone item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictEquityGrantMilestone> Apply(List<StrictEquityGrantMilestone> items)
    {
        var matched = Collections.Filter(items, (StrictEquityGrantMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict equity grant milestone store.</summary>
public static class StrictEquityGrantMilestoneStore
{
    public static IStrictEquityGrantMilestoneStore NewMemoryStrictEquityGrantMilestoneStore()
    {
        return new MemoryStrictEquityGrantMilestoneStore(
            new Dictionary<string, StrictEquityGrantMilestone>(),
            new List<string>());
    }

    public static IStrictEquityGrantMilestoneStore NewCachedStrictEquityGrantMilestoneStore(MemoryStrictEquityGrantMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedStrictEquityGrantMilestoneStore(
            inner,
            StrictEquityGrantMilestoneCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<StrictEquityGrantMilestone> FindStrictEquityGrantMilestones(IStrictEquityGrantMilestoneStore store, StrictEquityGrantMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
