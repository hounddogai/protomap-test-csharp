namespace Acme.Workforce.Contractors.Rate.RateBatch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate milestones by code.</summary>
public interface IRateMilestoneStore
{
    RateMilestone? Find(string code);
    void Save(RateMilestone item);
    bool Remove(string code);
    List<RateMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateMilestoneStore(Dictionary<string, RateMilestone> items, List<string> order) : IRateMilestoneStore
{
    public Dictionary<string, RateMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateMilestone item)
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

    public List<RateMilestone> List()
    {
        var outValue = new List<RateMilestone>();
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
public sealed class CachedRateMilestoneStore(MemoryRateMilestoneStore inner, RateMilestoneCache cache, int hits, int misses) : IRateMilestoneStore
{
    public MemoryRateMilestoneStore Inner { get; set; } = inner;
    public RateMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateMilestone? Find(string code)
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

    public void Save(RateMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate milestones. Empty fields match all.</summary>
public sealed record RateMilestoneQuery(string Department, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(RateMilestone item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RateMilestone> Apply(List<RateMilestone> items)
    {
        var matched = Collections.Filter(items, (RateMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate milestone store.</summary>
public static class RateMilestoneStore
{
    public static IRateMilestoneStore NewMemoryRateMilestoneStore()
    {
        return new MemoryRateMilestoneStore(new Dictionary<string, RateMilestone>(), new List<string>());
    }

    public static IRateMilestoneStore NewCachedRateMilestoneStore(MemoryRateMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedRateMilestoneStore(inner, RateMilestoneCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RateMilestone> FindRateMilestones(IRateMilestoneStore store, RateMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
