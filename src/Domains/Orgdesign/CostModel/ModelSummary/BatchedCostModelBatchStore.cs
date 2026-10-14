namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched cost model batches by code.</summary>
public interface IBatchedCostModelBatchStore
{
    BatchedCostModelBatch? Find(string code);
    void Save(BatchedCostModelBatch item);
    bool Remove(string code);
    List<BatchedCostModelBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedCostModelBatchStore(Dictionary<string, BatchedCostModelBatch> items, List<string> order) : IBatchedCostModelBatchStore
{
    public Dictionary<string, BatchedCostModelBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedCostModelBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedCostModelBatch item)
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

    public List<BatchedCostModelBatch> List()
    {
        var outValue = new List<BatchedCostModelBatch>();
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
public sealed class CachedBatchedCostModelBatchStore(MemoryBatchedCostModelBatchStore inner, BatchedCostModelBatchCache cache, int hits, int misses) : IBatchedCostModelBatchStore
{
    public MemoryBatchedCostModelBatchStore Inner { get; set; } = inner;
    public BatchedCostModelBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedCostModelBatch? Find(string code)
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

    public void Save(BatchedCostModelBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedCostModelBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched cost model batches. Empty fields match all.</summary>
public sealed record BatchedCostModelBatchQuery(string Department, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedCostModelBatch item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<BatchedCostModelBatch> Apply(List<BatchedCostModelBatch> items)
    {
        var matched = Collections.Filter(items, (BatchedCostModelBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched cost model batch store.</summary>
public static class BatchedCostModelBatchStore
{
    public static IBatchedCostModelBatchStore NewMemoryBatchedCostModelBatchStore()
    {
        return new MemoryBatchedCostModelBatchStore(
            new Dictionary<string, BatchedCostModelBatch>(),
            new List<string>());
    }

    public static IBatchedCostModelBatchStore NewCachedBatchedCostModelBatchStore(MemoryBatchedCostModelBatchStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedCostModelBatchStore(inner, BatchedCostModelBatchCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BatchedCostModelBatch> FindBatchedCostModelBatches(IBatchedCostModelBatchStore store, BatchedCostModelBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
