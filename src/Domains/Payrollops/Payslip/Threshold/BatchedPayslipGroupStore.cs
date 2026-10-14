namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched payslip groups by code.</summary>
public interface IBatchedPayslipGroupStore
{
    BatchedPayslipGroup? Find(string code);
    void Save(BatchedPayslipGroup item);
    bool Remove(string code);
    List<BatchedPayslipGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedPayslipGroupStore(Dictionary<string, BatchedPayslipGroup> items, List<string> order) : IBatchedPayslipGroupStore
{
    public Dictionary<string, BatchedPayslipGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedPayslipGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedPayslipGroup item)
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

    public List<BatchedPayslipGroup> List()
    {
        var outValue = new List<BatchedPayslipGroup>();
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
public sealed class CachedBatchedPayslipGroupStore(MemoryBatchedPayslipGroupStore inner, BatchedPayslipGroupCache cache, int hits, int misses) : IBatchedPayslipGroupStore
{
    public MemoryBatchedPayslipGroupStore Inner { get; set; } = inner;
    public BatchedPayslipGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedPayslipGroup? Find(string code)
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

    public void Save(BatchedPayslipGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedPayslipGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched payslip groups. Empty fields match all.</summary>
public sealed record BatchedPayslipGroupQuery(string Team, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedPayslipGroup item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<BatchedPayslipGroup> Apply(List<BatchedPayslipGroup> items)
    {
        var matched = Collections.Filter(items, (BatchedPayslipGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched payslip group store.</summary>
public static class BatchedPayslipGroupStore
{
    public static IBatchedPayslipGroupStore NewMemoryBatchedPayslipGroupStore()
    {
        return new MemoryBatchedPayslipGroupStore(new Dictionary<string, BatchedPayslipGroup>(), new List<string>());
    }

    public static IBatchedPayslipGroupStore NewCachedBatchedPayslipGroupStore(MemoryBatchedPayslipGroupStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedPayslipGroupStore(inner, BatchedPayslipGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BatchedPayslipGroup> FindBatchedPayslipGroups(IBatchedPayslipGroupStore store, BatchedPayslipGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
