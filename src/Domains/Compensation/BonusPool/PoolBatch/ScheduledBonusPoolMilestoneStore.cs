namespace Acme.Workforce.Compensation.BonusPool.PoolBatch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled bonus pool milestones by code.</summary>
public interface IScheduledBonusPoolMilestoneStore
{
    ScheduledBonusPoolMilestone? Find(string code);
    void Save(ScheduledBonusPoolMilestone item);
    bool Remove(string code);
    List<ScheduledBonusPoolMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledBonusPoolMilestoneStore(Dictionary<string, ScheduledBonusPoolMilestone> items, List<string> order) : IScheduledBonusPoolMilestoneStore
{
    public Dictionary<string, ScheduledBonusPoolMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledBonusPoolMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledBonusPoolMilestone item)
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

    public List<ScheduledBonusPoolMilestone> List()
    {
        var outValue = new List<ScheduledBonusPoolMilestone>();
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
public sealed class CachedScheduledBonusPoolMilestoneStore(MemoryScheduledBonusPoolMilestoneStore inner, ScheduledBonusPoolMilestoneCache cache, int hits, int misses) : IScheduledBonusPoolMilestoneStore
{
    public MemoryScheduledBonusPoolMilestoneStore Inner { get; set; } = inner;
    public ScheduledBonusPoolMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledBonusPoolMilestone? Find(string code)
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

    public void Save(ScheduledBonusPoolMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledBonusPoolMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled bonus pool milestones. Empty fields match all.</summary>
public sealed record ScheduledBonusPoolMilestoneQuery(string Name, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledBonusPoolMilestone item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledBonusPoolMilestone> Apply(List<ScheduledBonusPoolMilestone> items)
    {
        var matched = Collections.Filter(items, (ScheduledBonusPoolMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled bonus pool milestone store.</summary>
public static class ScheduledBonusPoolMilestoneStore
{
    public static IScheduledBonusPoolMilestoneStore NewMemoryScheduledBonusPoolMilestoneStore()
    {
        return new MemoryScheduledBonusPoolMilestoneStore(
            new Dictionary<string, ScheduledBonusPoolMilestone>(),
            new List<string>());
    }

    public static IScheduledBonusPoolMilestoneStore NewCachedScheduledBonusPoolMilestoneStore(MemoryScheduledBonusPoolMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledBonusPoolMilestoneStore(
            inner,
            ScheduledBonusPoolMilestoneCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledBonusPoolMilestone> FindScheduledBonusPoolMilestones(IScheduledBonusPoolMilestoneStore store, ScheduledBonusPoolMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
