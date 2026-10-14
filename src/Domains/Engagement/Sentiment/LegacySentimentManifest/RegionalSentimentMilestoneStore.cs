namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional sentiment milestones by code.</summary>
public interface IRegionalSentimentMilestoneStore
{
    RegionalSentimentMilestone? Find(string code);
    void Save(RegionalSentimentMilestone item);
    bool Remove(string code);
    List<RegionalSentimentMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalSentimentMilestoneStore(Dictionary<string, RegionalSentimentMilestone> items, List<string> order) : IRegionalSentimentMilestoneStore
{
    public Dictionary<string, RegionalSentimentMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalSentimentMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalSentimentMilestone item)
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

    public List<RegionalSentimentMilestone> List()
    {
        var outValue = new List<RegionalSentimentMilestone>();
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
public sealed class CachedRegionalSentimentMilestoneStore(MemoryRegionalSentimentMilestoneStore inner, RegionalSentimentMilestoneCache cache, int hits, int misses) : IRegionalSentimentMilestoneStore
{
    public MemoryRegionalSentimentMilestoneStore Inner { get; set; } = inner;
    public RegionalSentimentMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalSentimentMilestone? Find(string code)
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

    public void Save(RegionalSentimentMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalSentimentMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional sentiment milestones. Empty fields match all.</summary>
public sealed record RegionalSentimentMilestoneQuery(string Label, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalSentimentMilestone item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalSentimentMilestone> Apply(List<RegionalSentimentMilestone> items)
    {
        var matched = Collections.Filter(items, (RegionalSentimentMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional sentiment milestone store.</summary>
public static class RegionalSentimentMilestoneStore
{
    public static IRegionalSentimentMilestoneStore NewMemoryRegionalSentimentMilestoneStore()
    {
        return new MemoryRegionalSentimentMilestoneStore(
            new Dictionary<string, RegionalSentimentMilestone>(),
            new List<string>());
    }

    public static IRegionalSentimentMilestoneStore NewCachedRegionalSentimentMilestoneStore(MemoryRegionalSentimentMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalSentimentMilestoneStore(
            inner,
            RegionalSentimentMilestoneCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RegionalSentimentMilestone> FindRegionalSentimentMilestones(IRegionalSentimentMilestoneStore store, RegionalSentimentMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
