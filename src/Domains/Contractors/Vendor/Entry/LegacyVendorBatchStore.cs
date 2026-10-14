namespace Acme.Workforce.Contractors.Vendor.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy vendor batches by code.</summary>
public interface ILegacyVendorBatchStore
{
    LegacyVendorBatch? Find(string code);
    void Save(LegacyVendorBatch item);
    bool Remove(string code);
    List<LegacyVendorBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyVendorBatchStore(Dictionary<string, LegacyVendorBatch> items, List<string> order) : ILegacyVendorBatchStore
{
    public Dictionary<string, LegacyVendorBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyVendorBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyVendorBatch item)
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

    public List<LegacyVendorBatch> List()
    {
        var outValue = new List<LegacyVendorBatch>();
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
public sealed class CachedLegacyVendorBatchStore(MemoryLegacyVendorBatchStore inner, LegacyVendorBatchCache cache, int hits, int misses) : ILegacyVendorBatchStore
{
    public MemoryLegacyVendorBatchStore Inner { get; set; } = inner;
    public LegacyVendorBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyVendorBatch? Find(string code)
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

    public void Save(LegacyVendorBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyVendorBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy vendor batches. Empty fields match all.</summary>
public sealed record LegacyVendorBatchQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyVendorBatch item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyVendorBatch> Apply(List<LegacyVendorBatch> items)
    {
        var matched = Collections.Filter(items, (LegacyVendorBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy vendor batch store.</summary>
public static class LegacyVendorBatchStore
{
    public static ILegacyVendorBatchStore NewMemoryLegacyVendorBatchStore()
    {
        return new MemoryLegacyVendorBatchStore(new Dictionary<string, LegacyVendorBatch>(), new List<string>());
    }

    public static ILegacyVendorBatchStore NewCachedLegacyVendorBatchStore(MemoryLegacyVendorBatchStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyVendorBatchStore(inner, LegacyVendorBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LegacyVendorBatch> FindLegacyVendorBatches(ILegacyVendorBatchStore store, LegacyVendorBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
