namespace Acme.Workforce.Payrollops.Payslip.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched payslip proposals by code.</summary>
public interface IBatchedPayslipProposalStore
{
    BatchedPayslipProposal? Find(string code);
    void Save(BatchedPayslipProposal item);
    bool Remove(string code);
    List<BatchedPayslipProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedPayslipProposalStore(Dictionary<string, BatchedPayslipProposal> items, List<string> order) : IBatchedPayslipProposalStore
{
    public Dictionary<string, BatchedPayslipProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedPayslipProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedPayslipProposal item)
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

    public List<BatchedPayslipProposal> List()
    {
        var outValue = new List<BatchedPayslipProposal>();
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
public sealed class CachedBatchedPayslipProposalStore(MemoryBatchedPayslipProposalStore inner, BatchedPayslipProposalCache cache, int hits, int misses) : IBatchedPayslipProposalStore
{
    public MemoryBatchedPayslipProposalStore Inner { get; set; } = inner;
    public BatchedPayslipProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedPayslipProposal? Find(string code)
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

    public void Save(BatchedPayslipProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedPayslipProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched payslip proposals. Empty fields match all.</summary>
public sealed record BatchedPayslipProposalQuery(string Segment, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedPayslipProposal item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedPayslipProposal> Apply(List<BatchedPayslipProposal> items)
    {
        var matched = Collections.Filter(items, (BatchedPayslipProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched payslip proposal store.</summary>
public static class BatchedPayslipProposalStore
{
    public static IBatchedPayslipProposalStore NewMemoryBatchedPayslipProposalStore()
    {
        return new MemoryBatchedPayslipProposalStore(
            new Dictionary<string, BatchedPayslipProposal>(),
            new List<string>());
    }

    public static IBatchedPayslipProposalStore NewCachedBatchedPayslipProposalStore(MemoryBatchedPayslipProposalStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedPayslipProposalStore(
            inner,
            BatchedPayslipProposalCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<BatchedPayslipProposal> FindBatchedPayslipProposals(IBatchedPayslipProposalStore store, BatchedPayslipProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
