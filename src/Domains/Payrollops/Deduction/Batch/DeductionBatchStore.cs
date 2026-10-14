namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction batches by code.</summary>
public interface IDeductionBatchStore
{
    DeductionBatch? Find(string code);
    void Save(DeductionBatch item);
    bool Remove(string code);
    List<DeductionBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionBatchStore(Dictionary<string, DeductionBatch> items, List<string> order) : IDeductionBatchStore
{
    public Dictionary<string, DeductionBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionBatch item)
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

    public List<DeductionBatch> List()
    {
        var outValue = new List<DeductionBatch>();
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
public sealed class CachedDeductionBatchStore(MemoryDeductionBatchStore inner, DeductionBatchCache cache, int hits, int misses) : IDeductionBatchStore
{
    public MemoryDeductionBatchStore Inner { get; set; } = inner;
    public DeductionBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionBatch? Find(string code)
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

    public void Save(DeductionBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction batches. Empty fields match all.</summary>
public sealed record DeductionBatchQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionBatch item)
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

    public List<DeductionBatch> Apply(List<DeductionBatch> items)
    {
        var matched = Collections.Filter(items, (DeductionBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction batch store.</summary>
public static class DeductionBatchStore
{
    public static IDeductionBatchStore NewMemoryDeductionBatchStore()
    {
        return new MemoryDeductionBatchStore(new Dictionary<string, DeductionBatch>(), new List<string>());
    }

    public static IDeductionBatchStore NewCachedDeductionBatchStore(MemoryDeductionBatchStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionBatchStore(inner, DeductionBatchCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DeductionBatch> FindDeductionBatches(IDeductionBatchStore store, DeductionBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
