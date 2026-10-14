namespace Acme.Workforce.Contractors.Vendor.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor batches by code.</summary>
public interface IVendorBatchStore
{
    VendorBatch? Find(string code);
    void Save(VendorBatch item);
    bool Remove(string code);
    List<VendorBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorBatchStore(Dictionary<string, VendorBatch> items, List<string> order) : IVendorBatchStore
{
    public Dictionary<string, VendorBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorBatch item)
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

    public List<VendorBatch> List()
    {
        var outValue = new List<VendorBatch>();
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
public sealed class CachedVendorBatchStore(MemoryVendorBatchStore inner, VendorBatchCache cache, int hits, int misses) : IVendorBatchStore
{
    public MemoryVendorBatchStore Inner { get; set; } = inner;
    public VendorBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorBatch? Find(string code)
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

    public void Save(VendorBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor batches. Empty fields match all.</summary>
public sealed record VendorBatchQuery(string Label, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(VendorBatch item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorBatch> Apply(List<VendorBatch> items)
    {
        var matched = Collections.Filter(items, (VendorBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor batch store.</summary>
public static class VendorBatchStore
{
    public static IVendorBatchStore NewMemoryVendorBatchStore()
    {
        return new MemoryVendorBatchStore(new Dictionary<string, VendorBatch>(), new List<string>());
    }

    public static IVendorBatchStore NewCachedVendorBatchStore(MemoryVendorBatchStore inner, int ttl, IClock clock)
    {
        return new CachedVendorBatchStore(inner, VendorBatchCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<VendorBatch> FindVendorBatches(IVendorBatchStore store, VendorBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
