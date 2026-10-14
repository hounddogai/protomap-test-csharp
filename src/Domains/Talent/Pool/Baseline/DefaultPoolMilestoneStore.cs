namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default pool milestones by code.</summary>
public interface IDefaultPoolMilestoneStore
{
    DefaultPoolMilestone? Find(string code);
    void Save(DefaultPoolMilestone item);
    bool Remove(string code);
    List<DefaultPoolMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultPoolMilestoneStore(Dictionary<string, DefaultPoolMilestone> items, List<string> order) : IDefaultPoolMilestoneStore
{
    public Dictionary<string, DefaultPoolMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultPoolMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultPoolMilestone item)
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

    public List<DefaultPoolMilestone> List()
    {
        var outValue = new List<DefaultPoolMilestone>();
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
public sealed class CachedDefaultPoolMilestoneStore(MemoryDefaultPoolMilestoneStore inner, DefaultPoolMilestoneCache cache, int hits, int misses) : IDefaultPoolMilestoneStore
{
    public MemoryDefaultPoolMilestoneStore Inner { get; set; } = inner;
    public DefaultPoolMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultPoolMilestone? Find(string code)
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

    public void Save(DefaultPoolMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultPoolMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default pool milestones. Empty fields match all.</summary>
public sealed record DefaultPoolMilestoneQuery(string Region, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultPoolMilestone item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultPoolMilestone> Apply(List<DefaultPoolMilestone> items)
    {
        var matched = Collections.Filter(items, (DefaultPoolMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default pool milestone store.</summary>
public static class DefaultPoolMilestoneStore
{
    public static IDefaultPoolMilestoneStore NewMemoryDefaultPoolMilestoneStore()
    {
        return new MemoryDefaultPoolMilestoneStore(
            new Dictionary<string, DefaultPoolMilestone>(),
            new List<string>());
    }

    public static IDefaultPoolMilestoneStore NewCachedDefaultPoolMilestoneStore(MemoryDefaultPoolMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultPoolMilestoneStore(inner, DefaultPoolMilestoneCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<DefaultPoolMilestone> FindDefaultPoolMilestones(IDefaultPoolMilestoneStore store, DefaultPoolMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
