namespace Acme.Workforce.Assets.Warranty.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched warranty bundles by code.</summary>
public interface IBatchedWarrantyBundleStore
{
    BatchedWarrantyBundle? Find(string code);
    void Save(BatchedWarrantyBundle item);
    bool Remove(string code);
    List<BatchedWarrantyBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedWarrantyBundleStore(Dictionary<string, BatchedWarrantyBundle> items, List<string> order) : IBatchedWarrantyBundleStore
{
    public Dictionary<string, BatchedWarrantyBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedWarrantyBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedWarrantyBundle item)
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

    public List<BatchedWarrantyBundle> List()
    {
        var outValue = new List<BatchedWarrantyBundle>();
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
public sealed class CachedBatchedWarrantyBundleStore(MemoryBatchedWarrantyBundleStore inner, BatchedWarrantyBundleCache cache, int hits, int misses) : IBatchedWarrantyBundleStore
{
    public MemoryBatchedWarrantyBundleStore Inner { get; set; } = inner;
    public BatchedWarrantyBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedWarrantyBundle? Find(string code)
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

    public void Save(BatchedWarrantyBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedWarrantyBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched warranty bundles. Empty fields match all.</summary>
public sealed record BatchedWarrantyBundleQuery(string Department, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedWarrantyBundle item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<BatchedWarrantyBundle> Apply(List<BatchedWarrantyBundle> items)
    {
        var matched = Collections.Filter(items, (BatchedWarrantyBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched warranty bundle store.</summary>
public static class BatchedWarrantyBundleStore
{
    public static IBatchedWarrantyBundleStore NewMemoryBatchedWarrantyBundleStore()
    {
        return new MemoryBatchedWarrantyBundleStore(
            new Dictionary<string, BatchedWarrantyBundle>(),
            new List<string>());
    }

    public static IBatchedWarrantyBundleStore NewCachedBatchedWarrantyBundleStore(MemoryBatchedWarrantyBundleStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedWarrantyBundleStore(inner, BatchedWarrantyBundleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BatchedWarrantyBundle> FindBatchedWarrantyBundles(IBatchedWarrantyBundleStore store, BatchedWarrantyBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
