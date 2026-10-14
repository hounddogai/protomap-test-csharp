namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive action plan milestones by code.</summary>
public interface IAdaptiveActionPlanMilestoneStore
{
    AdaptiveActionPlanMilestone? Find(string code);
    void Save(AdaptiveActionPlanMilestone item);
    bool Remove(string code);
    List<AdaptiveActionPlanMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveActionPlanMilestoneStore(Dictionary<string, AdaptiveActionPlanMilestone> items, List<string> order) : IAdaptiveActionPlanMilestoneStore
{
    public Dictionary<string, AdaptiveActionPlanMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveActionPlanMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveActionPlanMilestone item)
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

    public List<AdaptiveActionPlanMilestone> List()
    {
        var outValue = new List<AdaptiveActionPlanMilestone>();
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
public sealed class CachedAdaptiveActionPlanMilestoneStore(MemoryAdaptiveActionPlanMilestoneStore inner, AdaptiveActionPlanMilestoneCache cache, int hits, int misses) : IAdaptiveActionPlanMilestoneStore
{
    public MemoryAdaptiveActionPlanMilestoneStore Inner { get; set; } = inner;
    public AdaptiveActionPlanMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveActionPlanMilestone? Find(string code)
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

    public void Save(AdaptiveActionPlanMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveActionPlanMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive action plan milestones. Empty fields match all.</summary>
public sealed record AdaptiveActionPlanMilestoneQuery(string Source, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveActionPlanMilestone item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveActionPlanMilestone> Apply(List<AdaptiveActionPlanMilestone> items)
    {
        var matched = Collections.Filter(items, (AdaptiveActionPlanMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive action plan milestone store.</summary>
public static class AdaptiveActionPlanMilestoneStore
{
    public static IAdaptiveActionPlanMilestoneStore NewMemoryAdaptiveActionPlanMilestoneStore()
    {
        return new MemoryAdaptiveActionPlanMilestoneStore(
            new Dictionary<string, AdaptiveActionPlanMilestone>(),
            new List<string>());
    }

    public static IAdaptiveActionPlanMilestoneStore NewCachedAdaptiveActionPlanMilestoneStore(MemoryAdaptiveActionPlanMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveActionPlanMilestoneStore(
            inner,
            AdaptiveActionPlanMilestoneCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveActionPlanMilestone> FindAdaptiveActionPlanMilestones(IAdaptiveActionPlanMilestoneStore store, AdaptiveActionPlanMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
