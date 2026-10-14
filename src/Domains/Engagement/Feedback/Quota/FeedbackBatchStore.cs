namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback batches by code.</summary>
public interface IFeedbackBatchStore
{
    FeedbackBatch? Find(string code);
    void Save(FeedbackBatch item);
    bool Remove(string code);
    List<FeedbackBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackBatchStore(Dictionary<string, FeedbackBatch> items, List<string> order) : IFeedbackBatchStore
{
    public Dictionary<string, FeedbackBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackBatch item)
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

    public List<FeedbackBatch> List()
    {
        var outValue = new List<FeedbackBatch>();
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
public sealed class CachedFeedbackBatchStore(MemoryFeedbackBatchStore inner, FeedbackBatchCache cache, int hits, int misses) : IFeedbackBatchStore
{
    public MemoryFeedbackBatchStore Inner { get; set; } = inner;
    public FeedbackBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackBatch? Find(string code)
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

    public void Save(FeedbackBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback batches. Empty fields match all.</summary>
public sealed record FeedbackBatchQuery(string Country, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackBatch item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<FeedbackBatch> Apply(List<FeedbackBatch> items)
    {
        var matched = Collections.Filter(items, (FeedbackBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback batch store.</summary>
public static class FeedbackBatchStore
{
    public static IFeedbackBatchStore NewMemoryFeedbackBatchStore()
    {
        return new MemoryFeedbackBatchStore(new Dictionary<string, FeedbackBatch>(), new List<string>());
    }

    public static IFeedbackBatchStore NewCachedFeedbackBatchStore(MemoryFeedbackBatchStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackBatchStore(inner, FeedbackBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<FeedbackBatch> FindFeedbackBatches(IFeedbackBatchStore store, FeedbackBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
