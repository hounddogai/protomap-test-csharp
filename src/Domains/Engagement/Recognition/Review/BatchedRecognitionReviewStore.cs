namespace Acme.Workforce.Engagement.Recognition.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched recognition reviews by code.</summary>
public interface IBatchedRecognitionReviewStore
{
    BatchedRecognitionReview? Find(string code);
    void Save(BatchedRecognitionReview item);
    bool Remove(string code);
    List<BatchedRecognitionReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedRecognitionReviewStore(Dictionary<string, BatchedRecognitionReview> items, List<string> order) : IBatchedRecognitionReviewStore
{
    public Dictionary<string, BatchedRecognitionReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedRecognitionReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedRecognitionReview item)
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

    public List<BatchedRecognitionReview> List()
    {
        var outValue = new List<BatchedRecognitionReview>();
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
public sealed class CachedBatchedRecognitionReviewStore(MemoryBatchedRecognitionReviewStore inner, BatchedRecognitionReviewCache cache, int hits, int misses) : IBatchedRecognitionReviewStore
{
    public MemoryBatchedRecognitionReviewStore Inner { get; set; } = inner;
    public BatchedRecognitionReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedRecognitionReview? Find(string code)
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

    public void Save(BatchedRecognitionReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedRecognitionReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched recognition reviews. Empty fields match all.</summary>
public sealed record BatchedRecognitionReviewQuery(string Name, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedRecognitionReview item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedRecognitionReview> Apply(List<BatchedRecognitionReview> items)
    {
        var matched = Collections.Filter(items, (BatchedRecognitionReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched recognition review store.</summary>
public static class BatchedRecognitionReviewStore
{
    public static IBatchedRecognitionReviewStore NewMemoryBatchedRecognitionReviewStore()
    {
        return new MemoryBatchedRecognitionReviewStore(
            new Dictionary<string, BatchedRecognitionReview>(),
            new List<string>());
    }

    public static IBatchedRecognitionReviewStore NewCachedBatchedRecognitionReviewStore(MemoryBatchedRecognitionReviewStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedRecognitionReviewStore(
            inner,
            BatchedRecognitionReviewCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedRecognitionReview> FindBatchedRecognitionReviews(IBatchedRecognitionReviewStore store, BatchedRecognitionReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
