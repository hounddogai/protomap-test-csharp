namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback entries by code.</summary>
public interface IFeedbackEntryStore
{
    FeedbackEntry? Find(string code);
    void Save(FeedbackEntry item);
    bool Remove(string code);
    List<FeedbackEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackEntryStore(Dictionary<string, FeedbackEntry> items, List<string> order) : IFeedbackEntryStore
{
    public Dictionary<string, FeedbackEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackEntry item)
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

    public List<FeedbackEntry> List()
    {
        var outValue = new List<FeedbackEntry>();
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
public sealed class CachedFeedbackEntryStore(MemoryFeedbackEntryStore inner, FeedbackEntryCache cache, int hits, int misses) : IFeedbackEntryStore
{
    public MemoryFeedbackEntryStore Inner { get; set; } = inner;
    public FeedbackEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackEntry? Find(string code)
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

    public void Save(FeedbackEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback entries. Empty fields match all.</summary>
public sealed record FeedbackEntryQuery(string Label, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackEntry item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FeedbackEntry> Apply(List<FeedbackEntry> items)
    {
        var matched = Collections.Filter(items, (FeedbackEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback entry store.</summary>
public static class FeedbackEntryStore
{
    public static IFeedbackEntryStore NewMemoryFeedbackEntryStore()
    {
        return new MemoryFeedbackEntryStore(new Dictionary<string, FeedbackEntry>(), new List<string>());
    }

    public static IFeedbackEntryStore NewCachedFeedbackEntryStore(MemoryFeedbackEntryStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackEntryStore(inner, FeedbackEntryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FeedbackEntry> FindFeedbackEntries(IFeedbackEntryStore store, FeedbackEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
