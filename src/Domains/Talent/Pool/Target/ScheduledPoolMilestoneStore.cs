namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled pool milestones by code.</summary>
public interface IScheduledPoolMilestoneStore
{
    ScheduledPoolMilestone? Find(string code);
    void Save(ScheduledPoolMilestone item);
    bool Remove(string code);
    List<ScheduledPoolMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledPoolMilestoneStore(Dictionary<string, ScheduledPoolMilestone> items, List<string> order) : IScheduledPoolMilestoneStore
{
    public Dictionary<string, ScheduledPoolMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledPoolMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledPoolMilestone item)
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

    public List<ScheduledPoolMilestone> List()
    {
        var outValue = new List<ScheduledPoolMilestone>();
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
public sealed class CachedScheduledPoolMilestoneStore(MemoryScheduledPoolMilestoneStore inner, ScheduledPoolMilestoneCache cache, int hits, int misses) : IScheduledPoolMilestoneStore
{
    public MemoryScheduledPoolMilestoneStore Inner { get; set; } = inner;
    public ScheduledPoolMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledPoolMilestone? Find(string code)
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

    public void Save(ScheduledPoolMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledPoolMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled pool milestones. Empty fields match all.</summary>
public sealed record ScheduledPoolMilestoneQuery(string Unit, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledPoolMilestone item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledPoolMilestone> Apply(List<ScheduledPoolMilestone> items)
    {
        var matched = Collections.Filter(items, (ScheduledPoolMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled pool milestone store.</summary>
public static class ScheduledPoolMilestoneStore
{
    public static IScheduledPoolMilestoneStore NewMemoryScheduledPoolMilestoneStore()
    {
        return new MemoryScheduledPoolMilestoneStore(
            new Dictionary<string, ScheduledPoolMilestone>(),
            new List<string>());
    }

    public static IScheduledPoolMilestoneStore NewCachedScheduledPoolMilestoneStore(MemoryScheduledPoolMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledPoolMilestoneStore(
            inner,
            ScheduledPoolMilestoneCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledPoolMilestone> FindScheduledPoolMilestones(IScheduledPoolMilestoneStore store, ScheduledPoolMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
