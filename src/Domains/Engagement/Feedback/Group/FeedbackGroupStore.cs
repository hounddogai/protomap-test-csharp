namespace Acme.Workforce.Engagement.Feedback.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback groups by code.</summary>
public interface IFeedbackGroupStore
{
    FeedbackGroup? Find(string code);
    void Save(FeedbackGroup item);
    bool Remove(string code);
    List<FeedbackGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackGroupStore(Dictionary<string, FeedbackGroup> items, List<string> order) : IFeedbackGroupStore
{
    public Dictionary<string, FeedbackGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackGroup item)
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

    public List<FeedbackGroup> List()
    {
        var outValue = new List<FeedbackGroup>();
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
public sealed class CachedFeedbackGroupStore(MemoryFeedbackGroupStore inner, FeedbackGroupCache cache, int hits, int misses) : IFeedbackGroupStore
{
    public MemoryFeedbackGroupStore Inner { get; set; } = inner;
    public FeedbackGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackGroup? Find(string code)
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

    public void Save(FeedbackGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback groups. Empty fields match all.</summary>
public sealed record FeedbackGroupQuery(string Category, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackGroup item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FeedbackGroup> Apply(List<FeedbackGroup> items)
    {
        var matched = Collections.Filter(items, (FeedbackGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback group store.</summary>
public static class FeedbackGroupStore
{
    public static IFeedbackGroupStore NewMemoryFeedbackGroupStore()
    {
        return new MemoryFeedbackGroupStore(new Dictionary<string, FeedbackGroup>(), new List<string>());
    }

    public static IFeedbackGroupStore NewCachedFeedbackGroupStore(MemoryFeedbackGroupStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackGroupStore(inner, FeedbackGroupCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FeedbackGroup> FindFeedbackGroups(IFeedbackGroupStore store, FeedbackGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
