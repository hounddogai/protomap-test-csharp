namespace Acme.Workforce.Engagement.Feedback.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback assignments by code.</summary>
public interface IFeedbackAssignmentStore
{
    FeedbackAssignment? Find(string code);
    void Save(FeedbackAssignment item);
    bool Remove(string code);
    List<FeedbackAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackAssignmentStore(Dictionary<string, FeedbackAssignment> items, List<string> order) : IFeedbackAssignmentStore
{
    public Dictionary<string, FeedbackAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackAssignment item)
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

    public List<FeedbackAssignment> List()
    {
        var outValue = new List<FeedbackAssignment>();
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
public sealed class CachedFeedbackAssignmentStore(MemoryFeedbackAssignmentStore inner, FeedbackAssignmentCache cache, int hits, int misses) : IFeedbackAssignmentStore
{
    public MemoryFeedbackAssignmentStore Inner { get; set; } = inner;
    public FeedbackAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackAssignment? Find(string code)
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

    public void Save(FeedbackAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback assignments. Empty fields match all.</summary>
public sealed record FeedbackAssignmentQuery(string Category, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackAssignment item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FeedbackAssignment> Apply(List<FeedbackAssignment> items)
    {
        var matched = Collections.Filter(items, (FeedbackAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback assignment store.</summary>
public static class FeedbackAssignmentStore
{
    public static IFeedbackAssignmentStore NewMemoryFeedbackAssignmentStore()
    {
        return new MemoryFeedbackAssignmentStore(new Dictionary<string, FeedbackAssignment>(), new List<string>());
    }

    public static IFeedbackAssignmentStore NewCachedFeedbackAssignmentStore(MemoryFeedbackAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackAssignmentStore(inner, FeedbackAssignmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<FeedbackAssignment> FindFeedbackAssignments(IFeedbackAssignmentStore store, FeedbackAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
