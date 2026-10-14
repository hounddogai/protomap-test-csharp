namespace Acme.Workforce.Contractors.Engagement.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched engagement exceptions by code.</summary>
public interface IBatchedEngagementExceptionStore
{
    BatchedEngagementException? Find(string code);
    void Save(BatchedEngagementException item);
    bool Remove(string code);
    List<BatchedEngagementException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedEngagementExceptionStore(Dictionary<string, BatchedEngagementException> items, List<string> order) : IBatchedEngagementExceptionStore
{
    public Dictionary<string, BatchedEngagementException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedEngagementException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedEngagementException item)
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

    public List<BatchedEngagementException> List()
    {
        var outValue = new List<BatchedEngagementException>();
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
public sealed class CachedBatchedEngagementExceptionStore(MemoryBatchedEngagementExceptionStore inner, BatchedEngagementExceptionCache cache, int hits, int misses) : IBatchedEngagementExceptionStore
{
    public MemoryBatchedEngagementExceptionStore Inner { get; set; } = inner;
    public BatchedEngagementExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedEngagementException? Find(string code)
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

    public void Save(BatchedEngagementException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedEngagementException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched engagement exceptions. Empty fields match all.</summary>
public sealed record BatchedEngagementExceptionQuery(string Department, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedEngagementException item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<BatchedEngagementException> Apply(List<BatchedEngagementException> items)
    {
        var matched = Collections.Filter(items, (BatchedEngagementException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched engagement exception store.</summary>
public static class BatchedEngagementExceptionStore
{
    public static IBatchedEngagementExceptionStore NewMemoryBatchedEngagementExceptionStore()
    {
        return new MemoryBatchedEngagementExceptionStore(
            new Dictionary<string, BatchedEngagementException>(),
            new List<string>());
    }

    public static IBatchedEngagementExceptionStore NewCachedBatchedEngagementExceptionStore(MemoryBatchedEngagementExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedEngagementExceptionStore(
            inner,
            BatchedEngagementExceptionCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedEngagementException> FindBatchedEngagementExceptions(IBatchedEngagementExceptionStore store, BatchedEngagementExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
