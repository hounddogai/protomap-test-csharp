namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched recognition slots by code.</summary>
public interface IBatchedRecognitionSlotStore
{
    BatchedRecognitionSlot? Find(string code);
    void Save(BatchedRecognitionSlot item);
    bool Remove(string code);
    List<BatchedRecognitionSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedRecognitionSlotStore(Dictionary<string, BatchedRecognitionSlot> items, List<string> order) : IBatchedRecognitionSlotStore
{
    public Dictionary<string, BatchedRecognitionSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedRecognitionSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedRecognitionSlot item)
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

    public List<BatchedRecognitionSlot> List()
    {
        var outValue = new List<BatchedRecognitionSlot>();
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
public sealed class CachedBatchedRecognitionSlotStore(MemoryBatchedRecognitionSlotStore inner, BatchedRecognitionSlotCache cache, int hits, int misses) : IBatchedRecognitionSlotStore
{
    public MemoryBatchedRecognitionSlotStore Inner { get; set; } = inner;
    public BatchedRecognitionSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedRecognitionSlot? Find(string code)
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

    public void Save(BatchedRecognitionSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedRecognitionSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched recognition slots. Empty fields match all.</summary>
public sealed record BatchedRecognitionSlotQuery(string Unit, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedRecognitionSlot item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<BatchedRecognitionSlot> Apply(List<BatchedRecognitionSlot> items)
    {
        var matched = Collections.Filter(items, (BatchedRecognitionSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched recognition slot store.</summary>
public static class BatchedRecognitionSlotStore
{
    public static IBatchedRecognitionSlotStore NewMemoryBatchedRecognitionSlotStore()
    {
        return new MemoryBatchedRecognitionSlotStore(
            new Dictionary<string, BatchedRecognitionSlot>(),
            new List<string>());
    }

    public static IBatchedRecognitionSlotStore NewCachedBatchedRecognitionSlotStore(MemoryBatchedRecognitionSlotStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedRecognitionSlotStore(
            inner,
            BatchedRecognitionSlotCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<BatchedRecognitionSlot> FindBatchedRecognitionSlots(IBatchedRecognitionSlotStore store, BatchedRecognitionSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
