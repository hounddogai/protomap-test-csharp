namespace Acme.Workforce.Assets.AssetReturn.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched asset return scores by code.</summary>
public interface IBatchedAssetReturnScoreStore
{
    BatchedAssetReturnScore? Find(string code);
    void Save(BatchedAssetReturnScore item);
    bool Remove(string code);
    List<BatchedAssetReturnScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedAssetReturnScoreStore(Dictionary<string, BatchedAssetReturnScore> items, List<string> order) : IBatchedAssetReturnScoreStore
{
    public Dictionary<string, BatchedAssetReturnScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedAssetReturnScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedAssetReturnScore item)
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

    public List<BatchedAssetReturnScore> List()
    {
        var outValue = new List<BatchedAssetReturnScore>();
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
public sealed class CachedBatchedAssetReturnScoreStore(MemoryBatchedAssetReturnScoreStore inner, BatchedAssetReturnScoreCache cache, int hits, int misses) : IBatchedAssetReturnScoreStore
{
    public MemoryBatchedAssetReturnScoreStore Inner { get; set; } = inner;
    public BatchedAssetReturnScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedAssetReturnScore? Find(string code)
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

    public void Save(BatchedAssetReturnScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedAssetReturnScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched asset return scores. Empty fields match all.</summary>
public sealed record BatchedAssetReturnScoreQuery(string Region, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedAssetReturnScore item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedAssetReturnScore> Apply(List<BatchedAssetReturnScore> items)
    {
        var matched = Collections.Filter(items, (BatchedAssetReturnScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched asset return score store.</summary>
public static class BatchedAssetReturnScoreStore
{
    public static IBatchedAssetReturnScoreStore NewMemoryBatchedAssetReturnScoreStore()
    {
        return new MemoryBatchedAssetReturnScoreStore(
            new Dictionary<string, BatchedAssetReturnScore>(),
            new List<string>());
    }

    public static IBatchedAssetReturnScoreStore NewCachedBatchedAssetReturnScoreStore(MemoryBatchedAssetReturnScoreStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedAssetReturnScoreStore(
            inner,
            BatchedAssetReturnScoreCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedAssetReturnScore> FindBatchedAssetReturnScores(IBatchedAssetReturnScoreStore store, BatchedAssetReturnScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
