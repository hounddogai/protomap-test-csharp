namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped payslip batches by code.</summary>
public interface ICappedPayslipBatchStore
{
    CappedPayslipBatch? Find(string code);
    void Save(CappedPayslipBatch item);
    bool Remove(string code);
    List<CappedPayslipBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedPayslipBatchStore(Dictionary<string, CappedPayslipBatch> items, List<string> order) : ICappedPayslipBatchStore
{
    public Dictionary<string, CappedPayslipBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedPayslipBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedPayslipBatch item)
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

    public List<CappedPayslipBatch> List()
    {
        var outValue = new List<CappedPayslipBatch>();
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
public sealed class CachedCappedPayslipBatchStore(MemoryCappedPayslipBatchStore inner, CappedPayslipBatchCache cache, int hits, int misses) : ICappedPayslipBatchStore
{
    public MemoryCappedPayslipBatchStore Inner { get; set; } = inner;
    public CappedPayslipBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedPayslipBatch? Find(string code)
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

    public void Save(CappedPayslipBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedPayslipBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped payslip batches. Empty fields match all.</summary>
public sealed record CappedPayslipBatchQuery(string Locale, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedPayslipBatch item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedPayslipBatch> Apply(List<CappedPayslipBatch> items)
    {
        var matched = Collections.Filter(items, (CappedPayslipBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped payslip batch store.</summary>
public static class CappedPayslipBatchStore
{
    public static ICappedPayslipBatchStore NewMemoryCappedPayslipBatchStore()
    {
        return new MemoryCappedPayslipBatchStore(new Dictionary<string, CappedPayslipBatch>(), new List<string>());
    }

    public static ICappedPayslipBatchStore NewCachedCappedPayslipBatchStore(MemoryCappedPayslipBatchStore inner, int ttl, IClock clock)
    {
        return new CachedCappedPayslipBatchStore(inner, CappedPayslipBatchCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CappedPayslipBatch> FindCappedPayslipBatches(ICappedPayslipBatchStore store, CappedPayslipBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
