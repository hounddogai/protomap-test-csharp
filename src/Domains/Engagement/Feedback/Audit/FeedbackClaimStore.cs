namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback claims by code.</summary>
public interface IFeedbackClaimStore
{
    FeedbackClaim? Find(string code);
    void Save(FeedbackClaim item);
    bool Remove(string code);
    List<FeedbackClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackClaimStore(Dictionary<string, FeedbackClaim> items, List<string> order) : IFeedbackClaimStore
{
    public Dictionary<string, FeedbackClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackClaim item)
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

    public List<FeedbackClaim> List()
    {
        var outValue = new List<FeedbackClaim>();
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
public sealed class CachedFeedbackClaimStore(MemoryFeedbackClaimStore inner, FeedbackClaimCache cache, int hits, int misses) : IFeedbackClaimStore
{
    public MemoryFeedbackClaimStore Inner { get; set; } = inner;
    public FeedbackClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackClaim? Find(string code)
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

    public void Save(FeedbackClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback claims. Empty fields match all.</summary>
public sealed record FeedbackClaimQuery(string Region, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackClaim item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<FeedbackClaim> Apply(List<FeedbackClaim> items)
    {
        var matched = Collections.Filter(items, (FeedbackClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback claim store.</summary>
public static class FeedbackClaimStore
{
    public static IFeedbackClaimStore NewMemoryFeedbackClaimStore()
    {
        return new MemoryFeedbackClaimStore(new Dictionary<string, FeedbackClaim>(), new List<string>());
    }

    public static IFeedbackClaimStore NewCachedFeedbackClaimStore(MemoryFeedbackClaimStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackClaimStore(inner, FeedbackClaimCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<FeedbackClaim> FindFeedbackClaims(IFeedbackClaimStore store, FeedbackClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
