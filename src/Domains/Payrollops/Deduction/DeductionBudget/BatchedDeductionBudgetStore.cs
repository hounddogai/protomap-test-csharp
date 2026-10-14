namespace Acme.Workforce.Payrollops.Deduction.DeductionBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched deduction budgets by code.</summary>
public interface IBatchedDeductionBudgetStore
{
    BatchedDeductionBudget? Find(string code);
    void Save(BatchedDeductionBudget item);
    bool Remove(string code);
    List<BatchedDeductionBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedDeductionBudgetStore(Dictionary<string, BatchedDeductionBudget> items, List<string> order) : IBatchedDeductionBudgetStore
{
    public Dictionary<string, BatchedDeductionBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedDeductionBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedDeductionBudget item)
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

    public List<BatchedDeductionBudget> List()
    {
        var outValue = new List<BatchedDeductionBudget>();
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
public sealed class CachedBatchedDeductionBudgetStore(MemoryBatchedDeductionBudgetStore inner, BatchedDeductionBudgetCache cache, int hits, int misses) : IBatchedDeductionBudgetStore
{
    public MemoryBatchedDeductionBudgetStore Inner { get; set; } = inner;
    public BatchedDeductionBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedDeductionBudget? Find(string code)
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

    public void Save(BatchedDeductionBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedDeductionBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched deduction budgets. Empty fields match all.</summary>
public sealed record BatchedDeductionBudgetQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedDeductionBudget item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedDeductionBudget> Apply(List<BatchedDeductionBudget> items)
    {
        var matched = Collections.Filter(items, (BatchedDeductionBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched deduction budget store.</summary>
public static class BatchedDeductionBudgetStore
{
    public static IBatchedDeductionBudgetStore NewMemoryBatchedDeductionBudgetStore()
    {
        return new MemoryBatchedDeductionBudgetStore(
            new Dictionary<string, BatchedDeductionBudget>(),
            new List<string>());
    }

    public static IBatchedDeductionBudgetStore NewCachedBatchedDeductionBudgetStore(MemoryBatchedDeductionBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedDeductionBudgetStore(
            inner,
            BatchedDeductionBudgetCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedDeductionBudget> FindBatchedDeductionBudgets(IBatchedDeductionBudgetStore store, BatchedDeductionBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
