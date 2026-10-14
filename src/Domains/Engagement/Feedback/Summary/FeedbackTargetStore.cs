namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback targets by code.</summary>
public interface IFeedbackTargetStore
{
    FeedbackTarget? Find(string code);
    void Save(FeedbackTarget item);
    bool Remove(string code);
    List<FeedbackTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackTargetStore(Dictionary<string, FeedbackTarget> items, List<string> order) : IFeedbackTargetStore
{
    public Dictionary<string, FeedbackTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackTarget item)
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

    public List<FeedbackTarget> List()
    {
        var outValue = new List<FeedbackTarget>();
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
public sealed class CachedFeedbackTargetStore(MemoryFeedbackTargetStore inner, FeedbackTargetCache cache, int hits, int misses) : IFeedbackTargetStore
{
    public MemoryFeedbackTargetStore Inner { get; set; } = inner;
    public FeedbackTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackTarget? Find(string code)
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

    public void Save(FeedbackTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback targets. Empty fields match all.</summary>
public sealed record FeedbackTargetQuery(string Country, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackTarget item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FeedbackTarget> Apply(List<FeedbackTarget> items)
    {
        var matched = Collections.Filter(items, (FeedbackTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback target store.</summary>
public static class FeedbackTargetStore
{
    public static IFeedbackTargetStore NewMemoryFeedbackTargetStore()
    {
        return new MemoryFeedbackTargetStore(new Dictionary<string, FeedbackTarget>(), new List<string>());
    }

    public static IFeedbackTargetStore NewCachedFeedbackTargetStore(MemoryFeedbackTargetStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackTargetStore(inner, FeedbackTargetCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<FeedbackTarget> FindFeedbackTargets(IFeedbackTargetStore store, FeedbackTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
