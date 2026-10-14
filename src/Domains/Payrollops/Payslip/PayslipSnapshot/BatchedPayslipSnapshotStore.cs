namespace Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched payslip snapshots by code.</summary>
public interface IBatchedPayslipSnapshotStore
{
    BatchedPayslipSnapshot? Find(string code);
    void Save(BatchedPayslipSnapshot item);
    bool Remove(string code);
    List<BatchedPayslipSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedPayslipSnapshotStore(Dictionary<string, BatchedPayslipSnapshot> items, List<string> order) : IBatchedPayslipSnapshotStore
{
    public Dictionary<string, BatchedPayslipSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedPayslipSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedPayslipSnapshot item)
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

    public List<BatchedPayslipSnapshot> List()
    {
        var outValue = new List<BatchedPayslipSnapshot>();
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
public sealed class CachedBatchedPayslipSnapshotStore(MemoryBatchedPayslipSnapshotStore inner, BatchedPayslipSnapshotCache cache, int hits, int misses) : IBatchedPayslipSnapshotStore
{
    public MemoryBatchedPayslipSnapshotStore Inner { get; set; } = inner;
    public BatchedPayslipSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedPayslipSnapshot? Find(string code)
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

    public void Save(BatchedPayslipSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedPayslipSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched payslip snapshots. Empty fields match all.</summary>
public sealed record BatchedPayslipSnapshotQuery(string Region, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedPayslipSnapshot item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedPayslipSnapshot> Apply(List<BatchedPayslipSnapshot> items)
    {
        var matched = Collections.Filter(items, (BatchedPayslipSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched payslip snapshot store.</summary>
public static class BatchedPayslipSnapshotStore
{
    public static IBatchedPayslipSnapshotStore NewMemoryBatchedPayslipSnapshotStore()
    {
        return new MemoryBatchedPayslipSnapshotStore(
            new Dictionary<string, BatchedPayslipSnapshot>(),
            new List<string>());
    }

    public static IBatchedPayslipSnapshotStore NewCachedBatchedPayslipSnapshotStore(MemoryBatchedPayslipSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedPayslipSnapshotStore(
            inner,
            BatchedPayslipSnapshotCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<BatchedPayslipSnapshot> FindBatchedPayslipSnapshots(IBatchedPayslipSnapshotStore store, BatchedPayslipSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
