namespace Acme.Workforce.Engagement.Feedback.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback checkpoints by code.</summary>
public interface IFeedbackCheckpointStore
{
    FeedbackCheckpoint? Find(string code);
    void Save(FeedbackCheckpoint item);
    bool Remove(string code);
    List<FeedbackCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackCheckpointStore(Dictionary<string, FeedbackCheckpoint> items, List<string> order) : IFeedbackCheckpointStore
{
    public Dictionary<string, FeedbackCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackCheckpoint item)
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

    public List<FeedbackCheckpoint> List()
    {
        var outValue = new List<FeedbackCheckpoint>();
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
public sealed class CachedFeedbackCheckpointStore(MemoryFeedbackCheckpointStore inner, FeedbackCheckpointCache cache, int hits, int misses) : IFeedbackCheckpointStore
{
    public MemoryFeedbackCheckpointStore Inner { get; set; } = inner;
    public FeedbackCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackCheckpoint? Find(string code)
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

    public void Save(FeedbackCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback checkpoints. Empty fields match all.</summary>
public sealed record FeedbackCheckpointQuery(string Locale, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackCheckpoint item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<FeedbackCheckpoint> Apply(List<FeedbackCheckpoint> items)
    {
        var matched = Collections.Filter(items, (FeedbackCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback checkpoint store.</summary>
public static class FeedbackCheckpointStore
{
    public static IFeedbackCheckpointStore NewMemoryFeedbackCheckpointStore()
    {
        return new MemoryFeedbackCheckpointStore(new Dictionary<string, FeedbackCheckpoint>(), new List<string>());
    }

    public static IFeedbackCheckpointStore NewCachedFeedbackCheckpointStore(MemoryFeedbackCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackCheckpointStore(inner, FeedbackCheckpointCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<FeedbackCheckpoint> FindFeedbackCheckpoints(IFeedbackCheckpointStore store, FeedbackCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
