namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback tiers by code.</summary>
public interface IFeedbackTierStore
{
    FeedbackTier? Find(string code);
    void Save(FeedbackTier item);
    bool Remove(string code);
    List<FeedbackTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackTierStore(Dictionary<string, FeedbackTier> items, List<string> order) : IFeedbackTierStore
{
    public Dictionary<string, FeedbackTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackTier item)
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

    public List<FeedbackTier> List()
    {
        var outValue = new List<FeedbackTier>();
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
public sealed class CachedFeedbackTierStore(MemoryFeedbackTierStore inner, FeedbackTierCache cache, int hits, int misses) : IFeedbackTierStore
{
    public MemoryFeedbackTierStore Inner { get; set; } = inner;
    public FeedbackTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackTier? Find(string code)
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

    public void Save(FeedbackTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback tiers. Empty fields match all.</summary>
public sealed record FeedbackTierQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackTier item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<FeedbackTier> Apply(List<FeedbackTier> items)
    {
        var matched = Collections.Filter(items, (FeedbackTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback tier store.</summary>
public static class FeedbackTierStore
{
    public static IFeedbackTierStore NewMemoryFeedbackTierStore()
    {
        return new MemoryFeedbackTierStore(new Dictionary<string, FeedbackTier>(), new List<string>());
    }

    public static IFeedbackTierStore NewCachedFeedbackTierStore(MemoryFeedbackTierStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackTierStore(inner, FeedbackTierCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<FeedbackTier> FindFeedbackTiers(IFeedbackTierStore store, FeedbackTierQuery query)
    {
        return query.Apply(store.List());
    }
}
