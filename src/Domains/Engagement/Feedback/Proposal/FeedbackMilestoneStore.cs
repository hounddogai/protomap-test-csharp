namespace Acme.Workforce.Engagement.Feedback.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback milestones by code.</summary>
public interface IFeedbackMilestoneStore
{
    FeedbackMilestone? Find(string code);
    void Save(FeedbackMilestone item);
    bool Remove(string code);
    List<FeedbackMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackMilestoneStore(Dictionary<string, FeedbackMilestone> items, List<string> order) : IFeedbackMilestoneStore
{
    public Dictionary<string, FeedbackMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackMilestone item)
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

    public List<FeedbackMilestone> List()
    {
        var outValue = new List<FeedbackMilestone>();
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
public sealed class CachedFeedbackMilestoneStore(MemoryFeedbackMilestoneStore inner, FeedbackMilestoneCache cache, int hits, int misses) : IFeedbackMilestoneStore
{
    public MemoryFeedbackMilestoneStore Inner { get; set; } = inner;
    public FeedbackMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackMilestone? Find(string code)
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

    public void Save(FeedbackMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback milestones. Empty fields match all.</summary>
public sealed record FeedbackMilestoneQuery(string Title, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackMilestone item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FeedbackMilestone> Apply(List<FeedbackMilestone> items)
    {
        var matched = Collections.Filter(items, (FeedbackMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback milestone store.</summary>
public static class FeedbackMilestoneStore
{
    public static IFeedbackMilestoneStore NewMemoryFeedbackMilestoneStore()
    {
        return new MemoryFeedbackMilestoneStore(new Dictionary<string, FeedbackMilestone>(), new List<string>());
    }

    public static IFeedbackMilestoneStore NewCachedFeedbackMilestoneStore(MemoryFeedbackMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackMilestoneStore(inner, FeedbackMilestoneCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<FeedbackMilestone> FindFeedbackMilestones(IFeedbackMilestoneStore store, FeedbackMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
