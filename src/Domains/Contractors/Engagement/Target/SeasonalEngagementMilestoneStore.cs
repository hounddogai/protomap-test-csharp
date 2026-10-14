namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal engagement milestones by code.</summary>
public interface ISeasonalEngagementMilestoneStore
{
    SeasonalEngagementMilestone? Find(string code);
    void Save(SeasonalEngagementMilestone item);
    bool Remove(string code);
    List<SeasonalEngagementMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalEngagementMilestoneStore(Dictionary<string, SeasonalEngagementMilestone> items, List<string> order) : ISeasonalEngagementMilestoneStore
{
    public Dictionary<string, SeasonalEngagementMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalEngagementMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalEngagementMilestone item)
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

    public List<SeasonalEngagementMilestone> List()
    {
        var outValue = new List<SeasonalEngagementMilestone>();
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
public sealed class CachedSeasonalEngagementMilestoneStore(MemorySeasonalEngagementMilestoneStore inner, SeasonalEngagementMilestoneCache cache, int hits, int misses) : ISeasonalEngagementMilestoneStore
{
    public MemorySeasonalEngagementMilestoneStore Inner { get; set; } = inner;
    public SeasonalEngagementMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalEngagementMilestone? Find(string code)
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

    public void Save(SeasonalEngagementMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalEngagementMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal engagement milestones. Empty fields match all.</summary>
public sealed record SeasonalEngagementMilestoneQuery(string Label, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalEngagementMilestone item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalEngagementMilestone> Apply(List<SeasonalEngagementMilestone> items)
    {
        var matched = Collections.Filter(items, (SeasonalEngagementMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal engagement milestone store.</summary>
public static class SeasonalEngagementMilestoneStore
{
    public static ISeasonalEngagementMilestoneStore NewMemorySeasonalEngagementMilestoneStore()
    {
        return new MemorySeasonalEngagementMilestoneStore(
            new Dictionary<string, SeasonalEngagementMilestone>(),
            new List<string>());
    }

    public static ISeasonalEngagementMilestoneStore NewCachedSeasonalEngagementMilestoneStore(MemorySeasonalEngagementMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalEngagementMilestoneStore(
            inner,
            SeasonalEngagementMilestoneCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalEngagementMilestone> FindSeasonalEngagementMilestones(ISeasonalEngagementMilestoneStore store, SeasonalEngagementMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
