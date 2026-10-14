namespace Acme.Workforce.Talent.Mobility.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched mobility slots by code.</summary>
public interface IBatchedMobilitySlotStore
{
    BatchedMobilitySlot? Find(string code);
    void Save(BatchedMobilitySlot item);
    bool Remove(string code);
    List<BatchedMobilitySlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedMobilitySlotStore(Dictionary<string, BatchedMobilitySlot> items, List<string> order) : IBatchedMobilitySlotStore
{
    public Dictionary<string, BatchedMobilitySlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedMobilitySlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedMobilitySlot item)
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

    public List<BatchedMobilitySlot> List()
    {
        var outValue = new List<BatchedMobilitySlot>();
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
public sealed class CachedBatchedMobilitySlotStore(MemoryBatchedMobilitySlotStore inner, BatchedMobilitySlotCache cache, int hits, int misses) : IBatchedMobilitySlotStore
{
    public MemoryBatchedMobilitySlotStore Inner { get; set; } = inner;
    public BatchedMobilitySlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedMobilitySlot? Find(string code)
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

    public void Save(BatchedMobilitySlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedMobilitySlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched mobility slots. Empty fields match all.</summary>
public sealed record BatchedMobilitySlotQuery(string Title, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedMobilitySlot item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedMobilitySlot> Apply(List<BatchedMobilitySlot> items)
    {
        var matched = Collections.Filter(items, (BatchedMobilitySlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched mobility slot store.</summary>
public static class BatchedMobilitySlotStore
{
    public static IBatchedMobilitySlotStore NewMemoryBatchedMobilitySlotStore()
    {
        return new MemoryBatchedMobilitySlotStore(new Dictionary<string, BatchedMobilitySlot>(), new List<string>());
    }

    public static IBatchedMobilitySlotStore NewCachedBatchedMobilitySlotStore(MemoryBatchedMobilitySlotStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedMobilitySlotStore(inner, BatchedMobilitySlotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BatchedMobilitySlot> FindBatchedMobilitySlots(IBatchedMobilitySlotStore store, BatchedMobilitySlotQuery query)
    {
        return query.Apply(store.List());
    }
}
