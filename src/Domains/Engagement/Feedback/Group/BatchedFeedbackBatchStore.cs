namespace Acme.Workforce.Engagement.Feedback.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched feedback batches by code.</summary>
public interface IBatchedFeedbackBatchStore
{
    BatchedFeedbackBatch? Find(string code);
    void Save(BatchedFeedbackBatch item);
    bool Remove(string code);
    List<BatchedFeedbackBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedFeedbackBatchStore(Dictionary<string, BatchedFeedbackBatch> items, List<string> order) : IBatchedFeedbackBatchStore
{
    public Dictionary<string, BatchedFeedbackBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedFeedbackBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedFeedbackBatch item)
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

    public List<BatchedFeedbackBatch> List()
    {
        var outValue = new List<BatchedFeedbackBatch>();
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
public sealed class CachedBatchedFeedbackBatchStore(MemoryBatchedFeedbackBatchStore inner, BatchedFeedbackBatchCache cache, int hits, int misses) : IBatchedFeedbackBatchStore
{
    public MemoryBatchedFeedbackBatchStore Inner { get; set; } = inner;
    public BatchedFeedbackBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedFeedbackBatch? Find(string code)
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

    public void Save(BatchedFeedbackBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedFeedbackBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched feedback batches. Empty fields match all.</summary>
public sealed record BatchedFeedbackBatchQuery(string Category, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedFeedbackBatch item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedFeedbackBatch> Apply(List<BatchedFeedbackBatch> items)
    {
        var matched = Collections.Filter(items, (BatchedFeedbackBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched feedback batch store.</summary>
public static class BatchedFeedbackBatchStore
{
    public static IBatchedFeedbackBatchStore NewMemoryBatchedFeedbackBatchStore()
    {
        return new MemoryBatchedFeedbackBatchStore(
            new Dictionary<string, BatchedFeedbackBatch>(),
            new List<string>());
    }

    public static IBatchedFeedbackBatchStore NewCachedBatchedFeedbackBatchStore(MemoryBatchedFeedbackBatchStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedFeedbackBatchStore(inner, BatchedFeedbackBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BatchedFeedbackBatch> FindBatchedFeedbackBatches(IBatchedFeedbackBatchStore store, BatchedFeedbackBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
