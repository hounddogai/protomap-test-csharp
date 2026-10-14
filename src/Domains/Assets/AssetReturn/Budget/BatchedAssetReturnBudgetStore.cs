namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched asset return budgets by code.</summary>
public interface IBatchedAssetReturnBudgetStore
{
    BatchedAssetReturnBudget? Find(string code);
    void Save(BatchedAssetReturnBudget item);
    bool Remove(string code);
    List<BatchedAssetReturnBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedAssetReturnBudgetStore(Dictionary<string, BatchedAssetReturnBudget> items, List<string> order) : IBatchedAssetReturnBudgetStore
{
    public Dictionary<string, BatchedAssetReturnBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedAssetReturnBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedAssetReturnBudget item)
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

    public List<BatchedAssetReturnBudget> List()
    {
        var outValue = new List<BatchedAssetReturnBudget>();
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
public sealed class CachedBatchedAssetReturnBudgetStore(MemoryBatchedAssetReturnBudgetStore inner, BatchedAssetReturnBudgetCache cache, int hits, int misses) : IBatchedAssetReturnBudgetStore
{
    public MemoryBatchedAssetReturnBudgetStore Inner { get; set; } = inner;
    public BatchedAssetReturnBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedAssetReturnBudget? Find(string code)
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

    public void Save(BatchedAssetReturnBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedAssetReturnBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched asset return budgets. Empty fields match all.</summary>
public sealed record BatchedAssetReturnBudgetQuery(string Segment, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedAssetReturnBudget item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedAssetReturnBudget> Apply(List<BatchedAssetReturnBudget> items)
    {
        var matched = Collections.Filter(items, (BatchedAssetReturnBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched asset return budget store.</summary>
public static class BatchedAssetReturnBudgetStore
{
    public static IBatchedAssetReturnBudgetStore NewMemoryBatchedAssetReturnBudgetStore()
    {
        return new MemoryBatchedAssetReturnBudgetStore(
            new Dictionary<string, BatchedAssetReturnBudget>(),
            new List<string>());
    }

    public static IBatchedAssetReturnBudgetStore NewCachedBatchedAssetReturnBudgetStore(MemoryBatchedAssetReturnBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedAssetReturnBudgetStore(
            inner,
            BatchedAssetReturnBudgetCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<BatchedAssetReturnBudget> FindBatchedAssetReturnBudgets(IBatchedAssetReturnBudgetStore store, BatchedAssetReturnBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
