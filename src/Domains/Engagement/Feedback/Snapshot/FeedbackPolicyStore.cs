namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback policies by code.</summary>
public interface IFeedbackPolicyStore
{
    FeedbackPolicy? Find(string code);
    void Save(FeedbackPolicy item);
    bool Remove(string code);
    List<FeedbackPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackPolicyStore(Dictionary<string, FeedbackPolicy> items, List<string> order) : IFeedbackPolicyStore
{
    public Dictionary<string, FeedbackPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackPolicy item)
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

    public List<FeedbackPolicy> List()
    {
        var outValue = new List<FeedbackPolicy>();
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
public sealed class CachedFeedbackPolicyStore(MemoryFeedbackPolicyStore inner, FeedbackPolicyCache cache, int hits, int misses) : IFeedbackPolicyStore
{
    public MemoryFeedbackPolicyStore Inner { get; set; } = inner;
    public FeedbackPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackPolicy? Find(string code)
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

    public void Save(FeedbackPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback policies. Empty fields match all.</summary>
public sealed record FeedbackPolicyQuery(string Locale, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackPolicy item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<FeedbackPolicy> Apply(List<FeedbackPolicy> items)
    {
        var matched = Collections.Filter(items, (FeedbackPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback policy store.</summary>
public static class FeedbackPolicyStore
{
    public static IFeedbackPolicyStore NewMemoryFeedbackPolicyStore()
    {
        return new MemoryFeedbackPolicyStore(new Dictionary<string, FeedbackPolicy>(), new List<string>());
    }

    public static IFeedbackPolicyStore NewCachedFeedbackPolicyStore(MemoryFeedbackPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackPolicyStore(inner, FeedbackPolicyCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<FeedbackPolicy> FindFeedbackPolicies(IFeedbackPolicyStore store, FeedbackPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
