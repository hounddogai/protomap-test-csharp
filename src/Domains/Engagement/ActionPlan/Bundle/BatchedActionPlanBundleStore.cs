namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched action plan bundles by code.</summary>
public interface IBatchedActionPlanBundleStore
{
    BatchedActionPlanBundle? Find(string code);
    void Save(BatchedActionPlanBundle item);
    bool Remove(string code);
    List<BatchedActionPlanBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedActionPlanBundleStore(Dictionary<string, BatchedActionPlanBundle> items, List<string> order) : IBatchedActionPlanBundleStore
{
    public Dictionary<string, BatchedActionPlanBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedActionPlanBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedActionPlanBundle item)
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

    public List<BatchedActionPlanBundle> List()
    {
        var outValue = new List<BatchedActionPlanBundle>();
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
public sealed class CachedBatchedActionPlanBundleStore(MemoryBatchedActionPlanBundleStore inner, BatchedActionPlanBundleCache cache, int hits, int misses) : IBatchedActionPlanBundleStore
{
    public MemoryBatchedActionPlanBundleStore Inner { get; set; } = inner;
    public BatchedActionPlanBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedActionPlanBundle? Find(string code)
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

    public void Save(BatchedActionPlanBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedActionPlanBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched action plan bundles. Empty fields match all.</summary>
public sealed record BatchedActionPlanBundleQuery(string Category, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedActionPlanBundle item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedActionPlanBundle> Apply(List<BatchedActionPlanBundle> items)
    {
        var matched = Collections.Filter(items, (BatchedActionPlanBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched action plan bundle store.</summary>
public static class BatchedActionPlanBundleStore
{
    public static IBatchedActionPlanBundleStore NewMemoryBatchedActionPlanBundleStore()
    {
        return new MemoryBatchedActionPlanBundleStore(
            new Dictionary<string, BatchedActionPlanBundle>(),
            new List<string>());
    }

    public static IBatchedActionPlanBundleStore NewCachedBatchedActionPlanBundleStore(MemoryBatchedActionPlanBundleStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedActionPlanBundleStore(
            inner,
            BatchedActionPlanBundleCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<BatchedActionPlanBundle> FindBatchedActionPlanBundles(IBatchedActionPlanBundleStore store, BatchedActionPlanBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
