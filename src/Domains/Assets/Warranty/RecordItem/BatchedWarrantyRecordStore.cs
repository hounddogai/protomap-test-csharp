namespace Acme.Workforce.Assets.Warranty.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched warranty records by code.</summary>
public interface IBatchedWarrantyRecordStore
{
    BatchedWarrantyRecord? Find(string code);
    void Save(BatchedWarrantyRecord item);
    bool Remove(string code);
    List<BatchedWarrantyRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedWarrantyRecordStore(Dictionary<string, BatchedWarrantyRecord> items, List<string> order) : IBatchedWarrantyRecordStore
{
    public Dictionary<string, BatchedWarrantyRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedWarrantyRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedWarrantyRecord item)
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

    public List<BatchedWarrantyRecord> List()
    {
        var outValue = new List<BatchedWarrantyRecord>();
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
public sealed class CachedBatchedWarrantyRecordStore(MemoryBatchedWarrantyRecordStore inner, BatchedWarrantyRecordCache cache, int hits, int misses) : IBatchedWarrantyRecordStore
{
    public MemoryBatchedWarrantyRecordStore Inner { get; set; } = inner;
    public BatchedWarrantyRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedWarrantyRecord? Find(string code)
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

    public void Save(BatchedWarrantyRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedWarrantyRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched warranty records. Empty fields match all.</summary>
public sealed record BatchedWarrantyRecordQuery(string Title, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedWarrantyRecord item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedWarrantyRecord> Apply(List<BatchedWarrantyRecord> items)
    {
        var matched = Collections.Filter(items, (BatchedWarrantyRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched warranty record store.</summary>
public static class BatchedWarrantyRecordStore
{
    public static IBatchedWarrantyRecordStore NewMemoryBatchedWarrantyRecordStore()
    {
        return new MemoryBatchedWarrantyRecordStore(
            new Dictionary<string, BatchedWarrantyRecord>(),
            new List<string>());
    }

    public static IBatchedWarrantyRecordStore NewCachedBatchedWarrantyRecordStore(MemoryBatchedWarrantyRecordStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedWarrantyRecordStore(
            inner,
            BatchedWarrantyRecordCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<BatchedWarrantyRecord> FindBatchedWarrantyRecords(IBatchedWarrantyRecordStore store, BatchedWarrantyRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
