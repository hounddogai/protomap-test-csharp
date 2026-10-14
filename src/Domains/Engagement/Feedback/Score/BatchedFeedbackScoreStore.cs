namespace Acme.Workforce.Engagement.Feedback.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched feedback scores by code.</summary>
public interface IBatchedFeedbackScoreStore
{
    BatchedFeedbackScore? Find(string code);
    void Save(BatchedFeedbackScore item);
    bool Remove(string code);
    List<BatchedFeedbackScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedFeedbackScoreStore(Dictionary<string, BatchedFeedbackScore> items, List<string> order) : IBatchedFeedbackScoreStore
{
    public Dictionary<string, BatchedFeedbackScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedFeedbackScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedFeedbackScore item)
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

    public List<BatchedFeedbackScore> List()
    {
        var outValue = new List<BatchedFeedbackScore>();
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
public sealed class CachedBatchedFeedbackScoreStore(MemoryBatchedFeedbackScoreStore inner, BatchedFeedbackScoreCache cache, int hits, int misses) : IBatchedFeedbackScoreStore
{
    public MemoryBatchedFeedbackScoreStore Inner { get; set; } = inner;
    public BatchedFeedbackScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedFeedbackScore? Find(string code)
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

    public void Save(BatchedFeedbackScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedFeedbackScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched feedback scores. Empty fields match all.</summary>
public sealed record BatchedFeedbackScoreQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedFeedbackScore item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedFeedbackScore> Apply(List<BatchedFeedbackScore> items)
    {
        var matched = Collections.Filter(items, (BatchedFeedbackScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched feedback score store.</summary>
public static class BatchedFeedbackScoreStore
{
    public static IBatchedFeedbackScoreStore NewMemoryBatchedFeedbackScoreStore()
    {
        return new MemoryBatchedFeedbackScoreStore(
            new Dictionary<string, BatchedFeedbackScore>(),
            new List<string>());
    }

    public static IBatchedFeedbackScoreStore NewCachedBatchedFeedbackScoreStore(MemoryBatchedFeedbackScoreStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedFeedbackScoreStore(inner, BatchedFeedbackScoreCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BatchedFeedbackScore> FindBatchedFeedbackScores(IBatchedFeedbackScoreStore store, BatchedFeedbackScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
