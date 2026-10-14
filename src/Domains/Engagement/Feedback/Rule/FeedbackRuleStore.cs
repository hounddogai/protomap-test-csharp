namespace Acme.Workforce.Engagement.Feedback.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback rules by code.</summary>
public interface IFeedbackRuleStore
{
    FeedbackRule? Find(string code);
    void Save(FeedbackRule item);
    bool Remove(string code);
    List<FeedbackRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackRuleStore(Dictionary<string, FeedbackRule> items, List<string> order) : IFeedbackRuleStore
{
    public Dictionary<string, FeedbackRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackRule item)
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

    public List<FeedbackRule> List()
    {
        var outValue = new List<FeedbackRule>();
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
public sealed class CachedFeedbackRuleStore(MemoryFeedbackRuleStore inner, FeedbackRuleCache cache, int hits, int misses) : IFeedbackRuleStore
{
    public MemoryFeedbackRuleStore Inner { get; set; } = inner;
    public FeedbackRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackRule? Find(string code)
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

    public void Save(FeedbackRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback rules. Empty fields match all.</summary>
public sealed record FeedbackRuleQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackRule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FeedbackRule> Apply(List<FeedbackRule> items)
    {
        var matched = Collections.Filter(items, (FeedbackRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback rule store.</summary>
public static class FeedbackRuleStore
{
    public static IFeedbackRuleStore NewMemoryFeedbackRuleStore()
    {
        return new MemoryFeedbackRuleStore(new Dictionary<string, FeedbackRule>(), new List<string>());
    }

    public static IFeedbackRuleStore NewCachedFeedbackRuleStore(MemoryFeedbackRuleStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackRuleStore(inner, FeedbackRuleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<FeedbackRule> FindFeedbackRules(IFeedbackRuleStore store, FeedbackRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
