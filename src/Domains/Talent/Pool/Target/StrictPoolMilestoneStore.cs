namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict pool milestones by code.</summary>
public interface IStrictPoolMilestoneStore
{
    StrictPoolMilestone? Find(string code);
    void Save(StrictPoolMilestone item);
    bool Remove(string code);
    List<StrictPoolMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictPoolMilestoneStore(Dictionary<string, StrictPoolMilestone> items, List<string> order) : IStrictPoolMilestoneStore
{
    public Dictionary<string, StrictPoolMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictPoolMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictPoolMilestone item)
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

    public List<StrictPoolMilestone> List()
    {
        var outValue = new List<StrictPoolMilestone>();
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
public sealed class CachedStrictPoolMilestoneStore(MemoryStrictPoolMilestoneStore inner, StrictPoolMilestoneCache cache, int hits, int misses) : IStrictPoolMilestoneStore
{
    public MemoryStrictPoolMilestoneStore Inner { get; set; } = inner;
    public StrictPoolMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictPoolMilestone? Find(string code)
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

    public void Save(StrictPoolMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictPoolMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict pool milestones. Empty fields match all.</summary>
public sealed record StrictPoolMilestoneQuery(string Channel, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(StrictPoolMilestone item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictPoolMilestone> Apply(List<StrictPoolMilestone> items)
    {
        var matched = Collections.Filter(items, (StrictPoolMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict pool milestone store.</summary>
public static class StrictPoolMilestoneStore
{
    public static IStrictPoolMilestoneStore NewMemoryStrictPoolMilestoneStore()
    {
        return new MemoryStrictPoolMilestoneStore(new Dictionary<string, StrictPoolMilestone>(), new List<string>());
    }

    public static IStrictPoolMilestoneStore NewCachedStrictPoolMilestoneStore(MemoryStrictPoolMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedStrictPoolMilestoneStore(inner, StrictPoolMilestoneCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<StrictPoolMilestone> FindStrictPoolMilestones(IStrictPoolMilestoneStore store, StrictPoolMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
