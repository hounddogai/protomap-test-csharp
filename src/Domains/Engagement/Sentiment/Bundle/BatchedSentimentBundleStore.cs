namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched sentiment bundles by code.</summary>
public interface IBatchedSentimentBundleStore
{
    BatchedSentimentBundle? Find(string code);
    void Save(BatchedSentimentBundle item);
    bool Remove(string code);
    List<BatchedSentimentBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedSentimentBundleStore(Dictionary<string, BatchedSentimentBundle> items, List<string> order) : IBatchedSentimentBundleStore
{
    public Dictionary<string, BatchedSentimentBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedSentimentBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedSentimentBundle item)
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

    public List<BatchedSentimentBundle> List()
    {
        var outValue = new List<BatchedSentimentBundle>();
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
public sealed class CachedBatchedSentimentBundleStore(MemoryBatchedSentimentBundleStore inner, BatchedSentimentBundleCache cache, int hits, int misses) : IBatchedSentimentBundleStore
{
    public MemoryBatchedSentimentBundleStore Inner { get; set; } = inner;
    public BatchedSentimentBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedSentimentBundle? Find(string code)
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

    public void Save(BatchedSentimentBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedSentimentBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched sentiment bundles. Empty fields match all.</summary>
public sealed record BatchedSentimentBundleQuery(string Label, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedSentimentBundle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedSentimentBundle> Apply(List<BatchedSentimentBundle> items)
    {
        var matched = Collections.Filter(items, (BatchedSentimentBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched sentiment bundle store.</summary>
public static class BatchedSentimentBundleStore
{
    public static IBatchedSentimentBundleStore NewMemoryBatchedSentimentBundleStore()
    {
        return new MemoryBatchedSentimentBundleStore(
            new Dictionary<string, BatchedSentimentBundle>(),
            new List<string>());
    }

    public static IBatchedSentimentBundleStore NewCachedBatchedSentimentBundleStore(MemoryBatchedSentimentBundleStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedSentimentBundleStore(
            inner,
            BatchedSentimentBundleCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<BatchedSentimentBundle> FindBatchedSentimentBundles(IBatchedSentimentBundleStore store, BatchedSentimentBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
