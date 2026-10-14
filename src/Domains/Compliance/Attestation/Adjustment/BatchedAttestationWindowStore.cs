namespace Acme.Workforce.Compliance.Attestation.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched attestation windows by code.</summary>
public interface IBatchedAttestationWindowStore
{
    BatchedAttestationWindow? Find(string code);
    void Save(BatchedAttestationWindow item);
    bool Remove(string code);
    List<BatchedAttestationWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedAttestationWindowStore(Dictionary<string, BatchedAttestationWindow> items, List<string> order) : IBatchedAttestationWindowStore
{
    public Dictionary<string, BatchedAttestationWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedAttestationWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedAttestationWindow item)
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

    public List<BatchedAttestationWindow> List()
    {
        var outValue = new List<BatchedAttestationWindow>();
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
public sealed class CachedBatchedAttestationWindowStore(MemoryBatchedAttestationWindowStore inner, BatchedAttestationWindowCache cache, int hits, int misses) : IBatchedAttestationWindowStore
{
    public MemoryBatchedAttestationWindowStore Inner { get; set; } = inner;
    public BatchedAttestationWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedAttestationWindow? Find(string code)
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

    public void Save(BatchedAttestationWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedAttestationWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched attestation windows. Empty fields match all.</summary>
public sealed record BatchedAttestationWindowQuery(string Source, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedAttestationWindow item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedAttestationWindow> Apply(List<BatchedAttestationWindow> items)
    {
        var matched = Collections.Filter(items, (BatchedAttestationWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched attestation window store.</summary>
public static class BatchedAttestationWindowStore
{
    public static IBatchedAttestationWindowStore NewMemoryBatchedAttestationWindowStore()
    {
        return new MemoryBatchedAttestationWindowStore(
            new Dictionary<string, BatchedAttestationWindow>(),
            new List<string>());
    }

    public static IBatchedAttestationWindowStore NewCachedBatchedAttestationWindowStore(MemoryBatchedAttestationWindowStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedAttestationWindowStore(
            inner,
            BatchedAttestationWindowCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<BatchedAttestationWindow> FindBatchedAttestationWindows(IBatchedAttestationWindowStore store, BatchedAttestationWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
