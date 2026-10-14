namespace Acme.Workforce.Engagement.Sentiment.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment milestones by code.</summary>
public interface ISentimentMilestoneStore
{
    SentimentMilestone? Find(string code);
    void Save(SentimentMilestone item);
    bool Remove(string code);
    List<SentimentMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentMilestoneStore(Dictionary<string, SentimentMilestone> items, List<string> order) : ISentimentMilestoneStore
{
    public Dictionary<string, SentimentMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentMilestone item)
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

    public List<SentimentMilestone> List()
    {
        var outValue = new List<SentimentMilestone>();
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
public sealed class CachedSentimentMilestoneStore(MemorySentimentMilestoneStore inner, SentimentMilestoneCache cache, int hits, int misses) : ISentimentMilestoneStore
{
    public MemorySentimentMilestoneStore Inner { get; set; } = inner;
    public SentimentMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentMilestone? Find(string code)
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

    public void Save(SentimentMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment milestones. Empty fields match all.</summary>
public sealed record SentimentMilestoneQuery(string Channel, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentMilestone item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentMilestone> Apply(List<SentimentMilestone> items)
    {
        var matched = Collections.Filter(items, (SentimentMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment milestone store.</summary>
public static class SentimentMilestoneStore
{
    public static ISentimentMilestoneStore NewMemorySentimentMilestoneStore()
    {
        return new MemorySentimentMilestoneStore(new Dictionary<string, SentimentMilestone>(), new List<string>());
    }

    public static ISentimentMilestoneStore NewCachedSentimentMilestoneStore(MemorySentimentMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentMilestoneStore(inner, SentimentMilestoneCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SentimentMilestone> FindSentimentMilestones(ISentimentMilestoneStore store, SentimentMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
