namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched laptop variances by code.</summary>
public interface IBatchedLaptopVarianceStore
{
    BatchedLaptopVariance? Find(string code);
    void Save(BatchedLaptopVariance item);
    bool Remove(string code);
    List<BatchedLaptopVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedLaptopVarianceStore(Dictionary<string, BatchedLaptopVariance> items, List<string> order) : IBatchedLaptopVarianceStore
{
    public Dictionary<string, BatchedLaptopVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedLaptopVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedLaptopVariance item)
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

    public List<BatchedLaptopVariance> List()
    {
        var outValue = new List<BatchedLaptopVariance>();
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
public sealed class CachedBatchedLaptopVarianceStore(MemoryBatchedLaptopVarianceStore inner, BatchedLaptopVarianceCache cache, int hits, int misses) : IBatchedLaptopVarianceStore
{
    public MemoryBatchedLaptopVarianceStore Inner { get; set; } = inner;
    public BatchedLaptopVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedLaptopVariance? Find(string code)
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

    public void Save(BatchedLaptopVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedLaptopVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched laptop variances. Empty fields match all.</summary>
public sealed record BatchedLaptopVarianceQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedLaptopVariance item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedLaptopVariance> Apply(List<BatchedLaptopVariance> items)
    {
        var matched = Collections.Filter(items, (BatchedLaptopVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched laptop variance store.</summary>
public static class BatchedLaptopVarianceStore
{
    public static IBatchedLaptopVarianceStore NewMemoryBatchedLaptopVarianceStore()
    {
        return new MemoryBatchedLaptopVarianceStore(
            new Dictionary<string, BatchedLaptopVariance>(),
            new List<string>());
    }

    public static IBatchedLaptopVarianceStore NewCachedBatchedLaptopVarianceStore(MemoryBatchedLaptopVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedLaptopVarianceStore(inner, BatchedLaptopVarianceCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BatchedLaptopVariance> FindBatchedLaptopVariances(IBatchedLaptopVarianceStore store, BatchedLaptopVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
