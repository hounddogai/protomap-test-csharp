namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global nomination milestones by code.</summary>
public interface IGlobalNominationMilestoneStore
{
    GlobalNominationMilestone? Find(string code);
    void Save(GlobalNominationMilestone item);
    bool Remove(string code);
    List<GlobalNominationMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalNominationMilestoneStore(Dictionary<string, GlobalNominationMilestone> items, List<string> order) : IGlobalNominationMilestoneStore
{
    public Dictionary<string, GlobalNominationMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalNominationMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalNominationMilestone item)
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

    public List<GlobalNominationMilestone> List()
    {
        var outValue = new List<GlobalNominationMilestone>();
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
public sealed class CachedGlobalNominationMilestoneStore(MemoryGlobalNominationMilestoneStore inner, GlobalNominationMilestoneCache cache, int hits, int misses) : IGlobalNominationMilestoneStore
{
    public MemoryGlobalNominationMilestoneStore Inner { get; set; } = inner;
    public GlobalNominationMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalNominationMilestone? Find(string code)
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

    public void Save(GlobalNominationMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalNominationMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global nomination milestones. Empty fields match all.</summary>
public sealed record GlobalNominationMilestoneQuery(string Label, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalNominationMilestone item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalNominationMilestone> Apply(List<GlobalNominationMilestone> items)
    {
        var matched = Collections.Filter(items, (GlobalNominationMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global nomination milestone store.</summary>
public static class GlobalNominationMilestoneStore
{
    public static IGlobalNominationMilestoneStore NewMemoryGlobalNominationMilestoneStore()
    {
        return new MemoryGlobalNominationMilestoneStore(
            new Dictionary<string, GlobalNominationMilestone>(),
            new List<string>());
    }

    public static IGlobalNominationMilestoneStore NewCachedGlobalNominationMilestoneStore(MemoryGlobalNominationMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalNominationMilestoneStore(
            inner,
            GlobalNominationMilestoneCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<GlobalNominationMilestone> FindGlobalNominationMilestones(IGlobalNominationMilestoneStore store, GlobalNominationMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
