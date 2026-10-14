namespace Acme.Workforce.Engagement.Sentiment.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched sentiment manifests by code.</summary>
public interface IBatchedSentimentManifestStore
{
    BatchedSentimentManifest? Find(string code);
    void Save(BatchedSentimentManifest item);
    bool Remove(string code);
    List<BatchedSentimentManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedSentimentManifestStore(Dictionary<string, BatchedSentimentManifest> items, List<string> order) : IBatchedSentimentManifestStore
{
    public Dictionary<string, BatchedSentimentManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedSentimentManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedSentimentManifest item)
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

    public List<BatchedSentimentManifest> List()
    {
        var outValue = new List<BatchedSentimentManifest>();
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
public sealed class CachedBatchedSentimentManifestStore(MemoryBatchedSentimentManifestStore inner, BatchedSentimentManifestCache cache, int hits, int misses) : IBatchedSentimentManifestStore
{
    public MemoryBatchedSentimentManifestStore Inner { get; set; } = inner;
    public BatchedSentimentManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedSentimentManifest? Find(string code)
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

    public void Save(BatchedSentimentManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedSentimentManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched sentiment manifests. Empty fields match all.</summary>
public sealed record BatchedSentimentManifestQuery(string Name, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedSentimentManifest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedSentimentManifest> Apply(List<BatchedSentimentManifest> items)
    {
        var matched = Collections.Filter(items, (BatchedSentimentManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched sentiment manifest store.</summary>
public static class BatchedSentimentManifestStore
{
    public static IBatchedSentimentManifestStore NewMemoryBatchedSentimentManifestStore()
    {
        return new MemoryBatchedSentimentManifestStore(
            new Dictionary<string, BatchedSentimentManifest>(),
            new List<string>());
    }

    public static IBatchedSentimentManifestStore NewCachedBatchedSentimentManifestStore(MemoryBatchedSentimentManifestStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedSentimentManifestStore(
            inner,
            BatchedSentimentManifestCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<BatchedSentimentManifest> FindBatchedSentimentManifests(IBatchedSentimentManifestStore store, BatchedSentimentManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
